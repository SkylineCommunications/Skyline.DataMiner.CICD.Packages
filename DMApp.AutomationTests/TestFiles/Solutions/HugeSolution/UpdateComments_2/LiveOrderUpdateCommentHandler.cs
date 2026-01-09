using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Comments;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.LoadingScreens;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;

namespace UpdateComments_2
{
	public class LiveOrderUpdateCommentHandler
	{
		private readonly Helpers helpers;
		private readonly Timer timer;
		private readonly InteractiveController interactiveController;

		private ProgressDialog progressDialog;
		private Order order;
		private UserInfo userInfo;

		private LoadLiveOrderUpdateCommentsDialog loadUpdateCommentsDialog;
		private UpdateCommentsDialog commentsDialog;

		private LiveOrderUpdateCommentHandler(Helpers helpers, InteractiveController interactiveController, Timer timer)
		{
			this.helpers = helpers;
			this.timer = timer;
			this.interactiveController = interactiveController;
		}

		public static void Run(Helpers helpers, InteractiveController interactiveController, Timer timer)
		{
			var handler = new LiveOrderUpdateCommentHandler(helpers, interactiveController, timer);

			handler.Run();
		}

		private void Run()
		{
			loadUpdateCommentsDialog = new LoadLiveOrderUpdateCommentsDialog(helpers, timer);
			if (loadUpdateCommentsDialog.Execute())
			{
				commentsDialog = loadUpdateCommentsDialog.UpdateCommentsDialog;
				order = loadUpdateCommentsDialog.Order;
				userInfo = loadUpdateCommentsDialog.UserInfo;

				commentsDialog.SaveButton.Pressed += CommentsDialog_SaveButton_Pressed;
				interactiveController.Run(commentsDialog);
			}
			else
			{
				interactiveController.Run(loadUpdateCommentsDialog);
			}		
		}

		private void CommentsDialog_SaveButton_Pressed(object sender, EventArgs e)
		{
			if (!commentsDialog.IsValid()) return;

			progressDialog = new ProgressDialog(helpers.Engine) { Title = "Updating Comments" };
			progressDialog.OkButton.Pressed += (sen, args) => helpers.Engine.ExitSuccess(order.Id.ToString());
			interactiveController.ShowDialog(progressDialog);

			UpdateComments();

			progressDialog.Finish();
			interactiveController.ShowDialog(progressDialog);
		}


		private bool UpdateComments()
		{
			bool success = true;

			// Order Operator Notes
			success &= TryUpdateMcrOperatorNotes();

			success &= TryUpdateMediaOperatorNotes();

			// Order Comments
			success &= TryUpdateComments();

			// Service Comments
			success &= TryUpdateServiceComments();

			// Update Last Update By property
			success &= TryUpdateLastModifiedBy();

			return success;
		}

		private bool TryUpdateMcrOperatorNotes()
		{
			if (order.McrOperatorNotes == commentsDialog.OrderMcrOperatorNotes || !userInfo.IsMcrUser) return true;

			order.McrOperatorNotes = commentsDialog.OrderMcrOperatorNotes;
			progressDialog.AddProgressLine("Updating Order MCR Operator Notes...");
			if (order.UpdateMcrOperatorNotesProperty(helpers))
			{
				order.UpdateUiProperties(helpers);

				progressDialog.AddProgressLine("Finished updating MCR Order Operator Notes");
				return true;
			}
			else
			{
				progressDialog.AddProgressLine("Updating Order MCR Operator Notes failed");
				return false;
			}
		}

		private bool TryUpdateMediaOperatorNotes()
		{
			if (order.MediaOperatorNotes == commentsDialog.OrderMediaOperatorNotes) return true;

			order.MediaOperatorNotes = commentsDialog.OrderMediaOperatorNotes;
			progressDialog.AddProgressLine("Updating Order Media Operator Notes...");
			if (order.UpdateMediaOperatorNotesProperty(helpers))
			{
				progressDialog.AddProgressLine("Finished updating Media Order Operator Notes");
				return true;
			}
			else
			{
				progressDialog.AddProgressLine("Updating Order Media Operator Notes failed");
				return false;
			}
		}

		private bool TryUpdateComments()
		{
			if (order.Comments == commentsDialog.OrderComments) return true;

			order.Comments = commentsDialog.OrderComments;
			progressDialog.AddProgressLine("Updating Order Comments...");
			if (order.UpdateCommentsProperty(helpers))
			{
				progressDialog.AddProgressLine("Finished updating Order Comments");
				return true;
			}
			else
			{
				progressDialog.AddProgressLine("Updating Order Comments failed");
				return false;
			}
		}

		private bool TryUpdateServiceComments()
		{
			bool success = true;
			bool updateServiceConfiguration = false;

			foreach (var service in order.AllServices)
			{
				if (service.IsUnknownSourceService)
				{
					UpdateUnknownSourceServiceComment(ref success, ref updateServiceConfiguration, service);
				}
				else if (service.Comments != commentsDialog.GetServiceComments(service.Id))
				{
					progressDialog.AddProgressLine("Updating comments on Service " + service.GetShortDescription(order) + "...");

					updateServiceConfiguration |= true;
					service.Comments = commentsDialog.GetServiceComments(service.Id);
					if (service.IsBooked && !service.UpdateCommentsProperty(helpers))
					{
						success &= false;
						progressDialog.AddProgressLine("Updating comments on Service " + service.GetShortDescription(order) + " failed");
					}
					else
					{
						success &= true;
						progressDialog.AddProgressLine("Finished Updating comments on Service");
					}
				}
				else
				{
					// nothing to do
				}
			}

			// Update Service Configuration on the Order
			if (updateServiceConfiguration)
			{
				success = UpdateServiceConfigurations(success);
			}

			return success;
		}

		private bool UpdateServiceConfigurations(bool success)
		{
			progressDialog.AddProgressLine("Updating Order...");

			if (order.UpdateServiceConfigurationProperty(helpers))
			{
				success &= true;
				progressDialog.AddProgressLine("Updating Order finished");
			}
			else
			{
				success &= false;
				progressDialog.AddProgressLine("Updating Order failed");
			}

			return success;
		}

		private void UpdateUnknownSourceServiceComment(ref bool success, ref bool updateServiceConfiguration, Service service)
		{
			string serviceComments = commentsDialog.GetServiceComments(service.Id);
			if (service.AdditionalDescriptionUnknownSource != serviceComments)
			{
				progressDialog.AddProgressLine("Updating comments on Service " + service.GetShortDescription(order) + "...");

				updateServiceConfiguration |= true;
				service.AdditionalDescriptionUnknownSource = serviceComments;
				success &= true;

				progressDialog.AddProgressLine("Finished Updating comments on Service");
			}
		}

		private bool TryUpdateLastModifiedBy()
		{
			if (order.LastUpdatedBy == helpers.Engine.UserLoginName) return true;

			order.LastUpdatedBy = helpers.Engine.UserLoginName;
			progressDialog.AddProgressLine("Updating Last Updated By property...");
			if (order.UpdateLastUpdatedByProperty(helpers))
			{
				progressDialog.AddProgressLine("Finished updating Last Updated By property");
				return true;
			}
			else
			{
				progressDialog.AddProgressLine("Updating Last Updated By property failed");
				return false;
			}
		}
	}
}
