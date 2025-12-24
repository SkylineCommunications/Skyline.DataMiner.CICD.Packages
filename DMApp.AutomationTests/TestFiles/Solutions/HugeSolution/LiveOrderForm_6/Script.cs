/*
****************************************************************************
*  Copyright (c) 2019,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

	Skyline Communications NV
	Ambachtenstraat 33
	B-8870 Izegem
	Belgium
	Tel.	: +32 51 31 35 69
	Fax.	: +32 51 31 01 29
	E-mail	: info@skyline.be
	Web		: www.skyline.be
	Contact	: Ben Vandenberghe

****************************************************************************
Revision History:

DATE		VERSION		AUTHOR			COMMENTS

dd/mm/2019	1.0.0.1		TRE, Skyline	Initial Version
22/03/2021	1.0.0.2		TRE, Skyline	Updated Audio Channel Configuration so that Audio Channels with type Other and different Other Descriptions are not marked as Stereo.
24/03/2021	1.0.0.3		TRE, Skyline	Updated form so MCR users can confirm Start Now orders
										Start Now is now available for Preliminary Orders
****************************************************************************
*/

namespace LiveOrderForm_6
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Timers;
	using Dialogs;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Contracts;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.OrderUpdates;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Recurrence;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.EventTasks;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using ExceptionDialog = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports.ExceptionDialog;
	using ReportDialog = Dialogs.ReportDialog;
	using RollBackReportDialog = Dialogs.RollBackReportDialog;
	using OrderStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.Status;
	using TaskStatus = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Status;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.History;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Locking;
	using Newtonsoft.Json;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Order.ExternalJson;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.YleWidgets;
	using Library_1.EventArguments;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Order.Dialogs;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
	using Skyline.DataMiner.Net.Messages;

	/// <summary>
	/// DataMiner Script Class.
	/// </summary>
	internal class Script : IDisposable
	{
		#region Fields
		private readonly Timer extendLocksTimer = new Timer();

		private Helpers helpers;
		private InteractiveController app;

		private LoadLiveOrderFormDialog loadLiveOrderFormDialog;
		private EventSelectionOrderDuplicationDialog eventSelectionOrderDuplicationDialog;
		private LoadLiveOrderFormDialogAfterOtherDialog loadLiveOrderFormDialogAfterOtherDialog;
		private OrderMergingDialog mergeOrdersDialog;
		private UseOrderTemplateDialog useOrderTemplateDialog;
		private LiveOrderFormDialog orderFormDialog;
		private EnclosingEventDeletionDialog eventDeletionDialog;
		private MultipleEventDeletionDialog multipleJobDeletionDialog;
		private ProvideReasonForStatusChangeDialog provideReasonForStatusChangeDialog;
		private AddOrUpdateReportDialog reportDialog;
		private DeleteOrderReportDialog deleteOrderDialog;
		private ReportDialog rollbackReportDialog;
		private AddOrderTemplateDialog addOrderTemplateDialog;
		private EditTemplateDialog editTemplateDialog;
		private SelectFileToUploadDialog selectFileToUploadDialog;
		private OrderHistoryDialog orderHistoryDialog;
		private ProgressDialog eurovisionBookingProgressDialog;

		private LiveOrderFormAction scriptAction;
		private string selectedOrderIdForExiting = Guid.Empty.ToString();
		private UserInfo userInfo;
		private List<LockInfo> lockInfos;

		private List<Order> receivedOrders;
		private Order receivedOrder;
		private Order mergedOrder;
		private List<Order> nonPrimaryMergingOrders;
		#endregion

		/// <summary>
		/// The Script entry point.
		/// </summary>
		/// <param name="engine">Link with SLScripting process.</param>
		public void Run(Engine engine)
		{
			int timeOutResult = 0;
			System.Threading.Tasks.Task scriptTask = System.Threading.Tasks.Task.Factory.StartNew(() =>
			{
				try
				{
					Initialize(engine);

					loadLiveOrderFormDialog = new LoadLiveOrderFormDialog(helpers, extendLocksTimer);

					if (loadLiveOrderFormDialog.Execute()) ShowLiveOrderForm();
					else app.Run(loadLiveOrderFormDialog);
				}
				catch (ScriptAbortException)
				{
					// Nothing to do
				}
				catch (InteractiveUserDetachedException)
				{
					// Nothing to do
				}
				catch (Exception e)
				{
					if (timeOutResult != -1)
					{
						helpers.Log(nameof(Script), nameof(Run), $"Something went wrong: {e}", receivedOrder?.Name);
						engine.Log("Run|Something went wrong: " + e);

						ShowExceptionDialog(engine, e);
					}
				}
				finally
				{
					Dispose();
				}
			});

			timeOutResult = System.Threading.Tasks.Task.WaitAny(new[] { scriptTask }, new TimeSpan(9, 59, 40));
			if (timeOutResult == -1) Dispose();
		}

		private void Initialize(Engine engine)
		{
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.SetFlag(RunTimeFlags.NoCheckingSets);
			engine.SetFlag(RunTimeFlags.NoInformationEvents);
			engine.Timeout = TimeSpan.FromHours(10);
			//engine.ShowUI();

			helpers = new Helpers(engine, Scripts.LiveOrderForm);

			app = new InteractiveController(engine);
		}

		private void ShowLiveOrderForm()
		{
			scriptAction = loadLiveOrderFormDialog.ScriptAction;
			receivedOrder = loadLiveOrderFormDialog.Orders.FirstOrDefault();
			receivedOrders = loadLiveOrderFormDialog.Orders;
			lockInfos = loadLiveOrderFormDialog.LockInfos;
			userInfo = loadLiveOrderFormDialog.UserInfo;

			Dialog dialog = null;
			switch (loadLiveOrderFormDialog.ScriptAction)
			{
				case LiveOrderFormAction.Add:
					orderFormDialog = loadLiveOrderFormDialog.EditOrderDialog;

					orderFormDialog.UploadJsonButtonPressed += OrderFormDialog_UploadJsonButtonPressed;
					orderFormDialog.UploadSynopsisButtonPressed += OrderFormDialog_UploadSynopsisButtonPressed;
					orderFormDialog.SaveOrderButton.Pressed += OrderForm_SaveButton_Pressed;
					orderFormDialog.BookOrderButton.Pressed += OrderForm_BookButton_Pressed;
					orderFormDialog.SaveAsTemplateButton.Pressed += SaveAsTemplateButton_Pressed;
					orderFormDialog.BookEurovisionService += (s, e) => BookEurovisionService(e);
					dialog = orderFormDialog;
					break;

				case LiveOrderFormAction.Edit when receivedOrder != null && !receivedOrder.RecurringSequenceInfo.Recurrence.IsConfigured:
					orderFormDialog = loadLiveOrderFormDialog.EditOrderDialog;

					orderFormDialog.UploadJsonButtonPressed += OrderFormDialog_UploadJsonButtonPressed;
					orderFormDialog.UploadSynopsisButtonPressed += OrderFormDialog_UploadSynopsisButtonPressed;
					orderFormDialog.SaveOrderButton.Pressed += OrderForm_SaveButton_Pressed;
					orderFormDialog.BookOrderButton.Pressed += OrderForm_BookButton_Pressed;
					orderFormDialog.SaveAsTemplateButton.Pressed += SaveAsTemplateButton_Pressed;
					orderFormDialog.CancelOrderButton.Pressed += (s, args) => ShowProvideReasonForStatusChangeDialog(OrderStatus.Cancelled);
					orderFormDialog.RejectButton.Pressed += (s, args) => ShowProvideReasonForStatusChangeDialog(OrderStatus.Rejected);
					orderFormDialog.ConfirmButton.Pressed += OrderForm_ConfirmButton_Pressed;
					orderFormDialog.StopOrderButton.Pressed += StopOrderOrServicesButton_Pressed;
					orderFormDialog.ExitButton.Pressed += (s, a) => helpers.Engine.ExitSuccess("Exit");
					orderFormDialog.BookEurovisionService += (s, e) => BookEurovisionService(e);
					orderFormDialog.SharedSourceUnavailableDueToOrderTimingChange += (s, e) => SharedSourceUnavailableDueToOrderTimingChange(e);
					orderFormDialog.HistoryButton.Pressed += HistoryButton_Pressed;

					dialog = orderFormDialog;
					break;

				case LiveOrderFormAction.Edit when receivedOrder != null && receivedOrder.RecurringSequenceInfo.Recurrence.IsConfigured:
					orderFormDialog = loadLiveOrderFormDialog.EditOrderDialog;

					orderFormDialog.UploadJsonButtonPressed += OrderFormDialog_UploadJsonButtonPressed;
					orderFormDialog.UploadSynopsisButtonPressed += OrderFormDialog_UploadSynopsisButtonPressed;
					orderFormDialog.SaveOrderButton.Pressed += OrderForm_SaveButton_Pressed;
					orderFormDialog.BookOrderButton.Pressed += OrderForm_BookButton_Pressed;
					orderFormDialog.SaveAsTemplateButton.Pressed += SaveAsTemplateButton_Pressed;
					orderFormDialog.CancelOrderButton.Pressed += (s, args) => ShowProvideReasonForStatusChangeDialog(OrderStatus.Cancelled);
					orderFormDialog.RejectButton.Pressed += (s, args) => ShowProvideReasonForStatusChangeDialog(OrderStatus.Rejected);
					orderFormDialog.ConfirmButton.Pressed += OrderForm_ConfirmButton_Pressed;
					orderFormDialog.StopOrderButton.Pressed += StopOrderOrServicesButton_Pressed;
					orderFormDialog.HistoryButton.Pressed += HistoryButton_Pressed;
					orderFormDialog.ExitButton.Pressed += (s, a) => helpers.Engine.ExitSuccess("Exit");
					orderFormDialog.BookEurovisionService += (s, e) => BookEurovisionService(e);

					var editRecurringOrderDialog = new EditRecurringOrderDialog(helpers.Engine);
					editRecurringOrderDialog.OkButtonPressed += (o, selectedAction) =>
					{
						orderFormDialog.Order.RecurringSequenceInfo.RecurrenceAction = selectedAction;
						app.ShowDialog(orderFormDialog);
					};

					dialog = editRecurringOrderDialog;

					break;

				case LiveOrderFormAction.Delete:
					var orderDeletionDialog = new OrderDeletionDialog(helpers.Engine, receivedOrder, lockInfos[0]);
					orderDeletionDialog.YesButton.Pressed += DeleteOrderForm_YesButton_Pressed;
					orderDeletionDialog.NoButton.Pressed += (s, args) => helpers.Engine.ExitSuccess("The Order was not deleted.");
					orderDeletionDialog.OkButton.Pressed += (s, args) => helpers.Engine.ExitSuccess("The Order was not deleted.");
					dialog = orderDeletionDialog;
					break;

				case LiveOrderFormAction.Duplicate:
					eventSelectionOrderDuplicationDialog = loadLiveOrderFormDialog.EventSelectionOrderDuplicationDialog;
					eventSelectionOrderDuplicationDialog.ConfirmButton.Pressed += EventSelectionConfirmButton_Pressed;
					dialog = eventSelectionOrderDuplicationDialog;
					break;

				case LiveOrderFormAction.Merge:
					mergeOrdersDialog = loadLiveOrderFormDialog.OrderMergingDialog;
					mergeOrdersDialog.EditMergedOrdersButton.Pressed += EditMergedOrdersButton_Pressed;
					dialog = mergeOrdersDialog;
					break;

				case LiveOrderFormAction.FromTemplate:
					if (loadLiveOrderFormDialog.UseOrderTemplateDialog.HasValidTemplates)
					{
						useOrderTemplateDialog = loadLiveOrderFormDialog.UseOrderTemplateDialog;
						useOrderTemplateDialog.ContinueButton.Pressed += UseOrderTemplateButton_Pressed;
						dialog = useOrderTemplateDialog;
					}
					else
					{
						var messageDialog = new MessageDialog(helpers.Engine, "You don't have access to any Order Templates.") { Title = "No Templates Available" };
						messageDialog.OkButton.Pressed += (s, a) => helpers.Engine.ExitSuccess("No Order Templates");
						dialog = messageDialog;
					}

					break;

				case LiveOrderFormAction.View:
					orderFormDialog = loadLiveOrderFormDialog.EditOrderDialog;
					orderFormDialog.HistoryButton.Pressed += HistoryButton_Pressed;
					orderFormDialog.ExitButton.Pressed += (s, a) => helpers.Engine.ExitSuccess("Exit");
					dialog = orderFormDialog;
					break;
				default:
					// Nothing to do
					break;
			}

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		private void HistoryButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			var currentDialog = app.CurrentDialog;

			if (orderHistoryDialog != null)
			{
				app.ShowDialog(orderHistoryDialog);
			}
			else if (helpers.OrderManagerElement.TryGetOrderHistory(receivedOrder.Id, out var orderHistory))
			{



				orderHistoryDialog = new OrderHistoryDialog(helpers, orderHistory);
				orderHistoryDialog.BackButton.Pressed += (s, a) => app.ShowDialog(currentDialog);

				app.ShowDialog(orderHistoryDialog);
			}
			else
			{
				var dialog = new MessageDialog(helpers.Engine, "No history available for this order") { Title = "Order History" };
				dialog.OkButton.Pressed += (s, a) => app.ShowDialog(currentDialog);

				app.ShowDialog(dialog);
			}
		}

		private void OrderFormDialog_UploadJsonButtonPressed(object sender, EventArgs e)
		{
			selectFileToUploadDialog = new SelectFileToUploadDialog(helpers.Engine);
			selectFileToUploadDialog.Cancelled += (o, ee) => app.ShowDialog(orderFormDialog);
			selectFileToUploadDialog.Confirmed += (o, filePaths) =>
			{
				string serializedJson = System.IO.File.ReadAllText(filePaths.FirstOrDefault());

				var externalJson = JsonConvert.DeserializeObject<ExternalJson>(serializedJson);

				var newOrder = Order.FromExternalJson(helpers, externalJson);

				newOrder.SetSecurityViewIds(userInfo.UserGroups.Select(u => u.CompanySecurityViewId).Distinct());
				newOrder.SetUserGroupIds(userInfo.UserGroups.Select(u => Convert.ToInt32(u.ID)).Distinct());
				newOrder.Event = loadLiveOrderFormDialog.Events.FirstOrDefault();

				foreach (var source in newOrder.Sources)
				{
					source.SecurityViewIds = new HashSet<int>(newOrder.SecurityViewIds);
				}

				loadLiveOrderFormDialogAfterOtherDialog = new LoadLiveOrderFormDialogAfterOtherDialog(helpers, newOrder, null, userInfo, scriptAction);

				if (loadLiveOrderFormDialogAfterOtherDialog.Execute()) ShowLiveOrderFormAfterOtherDialog();
				else app.ShowDialog(loadLiveOrderFormDialogAfterOtherDialog);
			};

			app.ShowDialog(selectFileToUploadDialog);
		}

		private void OrderFormDialog_UploadSynopsisButtonPressed(object sender, ServiceEventArgs args)
		{
			selectFileToUploadDialog = new SelectFileToUploadDialog(helpers.Engine)
			{
				Info = "Please select a file containing the synopsis for the Satellite Reception.",
				AllowMultipleFiles = true
			};

			selectFileToUploadDialog.Cancelled += (o, ee) => app.ShowDialog(orderFormDialog);
			selectFileToUploadDialog.Confirmed += (o, filePaths) =>
			{
				foreach (string filePath in filePaths) args.Service.SynopsisFiles.Add(filePath);
				app.ShowDialog(orderFormDialog);
			};

			app.ShowDialog(selectFileToUploadDialog);
		}

		private void BookEurovisionService(ServiceEventArgs args)
		{
			var service = args.Service;

			helpers.Log(nameof(Script), nameof(BookEurovisionService), JsonConvert.SerializeObject(service.EurovisionBookingDetails));

			eurovisionBookingProgressDialog = new ProgressDialog(helpers.Engine) { Title = "Create Eurovision Booking" };
			eurovisionBookingProgressDialog.OkButton.Pressed += (s, e) => app.ShowDialog(orderFormDialog);

			EurovisionBookingHandler handler = new EurovisionBookingHandler(helpers, ((NormalOrderSection)orderFormDialog.OrderSection).GeneralInfoSection.UserGroup);
			handler.ProgressReported += (s, e) => eurovisionBookingProgressDialog.AddProgressLine(e);

			app.ShowDialog(eurovisionBookingProgressDialog);

			EurovisionBookingResult result = new EurovisionBookingResult { IsSuccessful = false };
			try
			{
				result = handler.BookEurovisionService(service);
			}
			catch (Exception e)
			{
				eurovisionBookingProgressDialog.AddProgressLine($"Something went wrong with booking the Eurovision order. {e}");
			}

			if (result.IsSuccessful) service.EurovisionWorkOrderId = result.Id;

			eurovisionBookingProgressDialog.Finish();
			app.ShowDialog(eurovisionBookingProgressDialog);
		}

		private void SharedSourceUnavailableDueToOrderTimingChange(ServiceEventArgs args)
		{
			var messageDialog = new MessageDialog(helpers.Engine, $"Selected Shared Source {args.Service.GetShortDescription()} became unavailable due to changing order timing.") { Title = "Shared Source is Unavailable" };
			messageDialog.OkButton.Pressed += (s, e) => app.ShowDialog(orderFormDialog);

			app.ShowDialog(messageDialog);
		}

		private void StopOrderOrServicesButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			try
			{
				ConfirmStopDialog confirmStopDialog = new ConfirmStopDialog(helpers.Engine);
				confirmStopDialog.NoButton.Pressed += (o, args) => app.ShowDialog(orderFormDialog);
				confirmStopDialog.YesButton.Pressed += ConfirmStopDialog_YesButton_Pressed;

				app.ShowDialog(confirmStopDialog);
			}
			catch (Exception ex)
			{
				ShowExceptionDialog(helpers.Engine, ex);
			}
		}

		private void ConfirmStopDialog_YesButton_Pressed(object sender, EventArgs e)
		{
			StopOrder();
		}

		private void StopOrder()
		{
			reportDialog = new AddOrUpdateReportDialog(helpers, userInfo.IsMcrUser) { Title = "Stop Order" };
			reportDialog.RollBackButton.IsVisible = false;
			reportDialog.OkButton.Pressed += (o, args) => { helpers.LockManager.ReleaseLocks(); helpers.Engine.ExitSuccess("Order and linked services are stopped."); };
			app.ShowDialog(reportDialog);

			helpers.ReportProgress("Start executing stop functionality ...");

			if (receivedOrder != null)
			{
				receivedOrder.StopNow = true;

				var result = receivedOrder.StopOrderAndLinkedServices(helpers);

				if (result.Tasks.Any())
				{
					reportDialog.Finish(result);

					// release the locks if all tasks were successful
					if (result.Tasks.All(t => t.Status == TaskStatus.Ok)) helpers.LockManager.ReleaseLocks();

					app.ShowDialog(reportDialog);
				}
				else ShowMessageDialog(helpers.Engine, "No changes made", "No Changes made");
			}
			else
			{
				helpers.Log(nameof(Script), nameof(StopOrder), "Received order doesn't exist.");
			}
		}

		private void OrderForm_ConfirmButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			helpers.LogMethodStart(nameof(Script), nameof(OrderForm_ConfirmButton_Pressed), out var stopwatch);

			try
			{
				if (!orderFormDialog.IsValid(false, false, true))
				{
					helpers.LogMethodCompleted(nameof(Script), nameof(OrderForm_ConfirmButton_Pressed), null, stopwatch);
					return;
				}

				orderFormDialog.UnsubscribeFromUi();

				ConfirmAndUpdateOrder();
			}
			catch (Exception exception)
			{
				ShowExceptionDialog(helpers.Engine, exception);
			}

			helpers.LogMethodCompleted(nameof(Script), nameof(OrderForm_ConfirmButton_Pressed), null, stopwatch);
		}

		private void ConfirmAndUpdateOrder()
		{
			reportDialog = new AddOrUpdateReportDialog(helpers, userInfo.IsMcrUser) { Title = "Updating Order" };
			reportDialog.OkButton.Pressed += ReportDialog_OkButton_Pressed;
			reportDialog.RollBackButton.Pressed += ReportDialog_RollbackButton_Pressed;
			app.ShowDialog(reportDialog);

			orderFormDialog.Order.Status = OrderStatus.Confirmed;

			helpers.Log(nameof(Script), nameof(ConfirmAndUpdateOrder), $"Set order status to {OrderStatus.Confirmed}");

			var tasks = orderFormDialog.Order.AddOrUpdate(helpers, userInfo.IsMcrUser);

			// Selection of which Id is filled in inside the engine.ExitSuccess() to support correct details panel behavior.
			selectedOrderIdForExiting = SelectIdForExiting(orderFormDialog.Order);

			reportDialog.Finish(tasks);

			// release the locks if all tasks were successful, this way background process can immediately get the lock
			if (tasks.Tasks.All(t => t.Status == TaskStatus.Ok)) ReleaseLocks();

			app.ShowDialog(reportDialog);
		}

		private void EventSelectionConfirmButton_Pressed(object sender, EventArgs e)
		{
			try
			{
				if (!eventSelectionOrderDuplicationDialog.IsValid()) return;

				receivedOrder.Event = eventSelectionOrderDuplicationDialog.SelectedEvent;
				eventSelectionOrderDuplicationDialog.UpdateRecordingNamesBasedOnOrderName();

				// If an MCR user selects another existing event from another company, the wrong contract is used.
				// Also if a user is not part of the original orders company and wants to create a new Event, the wrong contract is used.
				if (eventSelectionOrderDuplicationDialog.Selection == EventSelection.NewEvent || eventSelectionOrderDuplicationDialog.Selection == EventSelection.OtherExistingEvent)
				{
					userInfo = helpers.ContractManager.GetUserInfo(helpers.Engine.UserLoginName, receivedOrder.Event);
				}

				LockInfo lockInfo;
				if (eventSelectionOrderDuplicationDialog.Selection == EventSelection.NewEvent)
				{
					lockInfo = new LockInfo(true, String.Empty, String.Empty, TimeSpan.Zero);
				}
				else
				{
					lockInfo = lockInfos.Single();
				}

				loadLiveOrderFormDialogAfterOtherDialog = new LoadLiveOrderFormDialogAfterOtherDialog(helpers, receivedOrder, lockInfo, userInfo, scriptAction);

				if (loadLiveOrderFormDialogAfterOtherDialog.Execute()) ShowLiveOrderFormAfterOtherDialog();
				else app.ShowDialog(loadLiveOrderFormDialogAfterOtherDialog);
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(Script), nameof(EventSelectionConfirmButton_Pressed), $"Exception occurred: {ex}");
				throw;
			}
		}

		private void ShowLiveOrderFormAfterOtherDialog()
		{
			orderFormDialog = loadLiveOrderFormDialogAfterOtherDialog.EditOrderDialog;

			orderFormDialog.CancelOrderButton.IsVisible = false;
			orderFormDialog.RejectButton.IsVisible = false;
			orderFormDialog.ConfirmButton.IsVisible = false;

			orderFormDialog.SaveOrderButton.Pressed += OrderForm_SaveButton_Pressed;
			orderFormDialog.BookOrderButton.Pressed += OrderForm_BookButton_Pressed;

			app.ShowDialog(orderFormDialog);
		}

		private void EditMergedOrdersButton_Pressed(object sender, EventArgs e)
		{
			try
			{
				if (!mergeOrdersDialog.IsValid()) return;

				mergedOrder = mergeOrdersDialog.GetMergedOrder();

				nonPrimaryMergingOrders = mergeOrdersDialog.GetNonPrimaryMergingOrders();

				loadLiveOrderFormDialogAfterOtherDialog = new LoadLiveOrderFormDialogAfterOtherDialog(helpers, mergedOrder, lockInfos.First(l => l.ObjectId == mergedOrder.Id.ToString()), userInfo, scriptAction);

				if (loadLiveOrderFormDialogAfterOtherDialog.Execute()) ShowLiveOrderFormAfterMerge();
				else app.ShowDialog(loadLiveOrderFormDialogAfterOtherDialog);
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(Script), nameof(EditMergedOrdersButton_Pressed), ex.ToString());
				throw;
			}
		}

		private void UseOrderTemplateButton_Pressed(object sender, EventArgs e)
		{
			if (!useOrderTemplateDialog.IsValid) return;

			try
			{
				OrderTemplate template = useOrderTemplateDialog.SelectedTemplate;
				string orderName = useOrderTemplateDialog.OrderName;
				DateTime startTime = useOrderTemplateDialog.StartTime;

				Order order = Order.FromTemplate(helpers, template, orderName, startTime);

				loadLiveOrderFormDialogAfterOtherDialog = new LoadLiveOrderFormDialogAfterOtherDialog(helpers, order, loadLiveOrderFormDialog.LockInfos.FirstOrDefault() ?? new LockInfo(true, String.Empty, String.Empty, TimeSpan.Zero), userInfo, scriptAction);

				if (loadLiveOrderFormDialogAfterOtherDialog.Execute()) ShowLiveOrderFormAfterChoosingTemplate();
				else app.ShowDialog(loadLiveOrderFormDialogAfterOtherDialog);
			}
			catch (Exception ex)
			{
				helpers.Log(nameof(Script), nameof(UseOrderTemplateButton_Pressed), $"Exception occurred: {ex}");
				throw;
			}
		}

		private void ShowLiveOrderFormAfterMerge()
		{
			orderFormDialog = loadLiveOrderFormDialogAfterOtherDialog.EditOrderDialog;
			orderFormDialog.ConfirmButton.IsVisible = false;
			orderFormDialog.RejectButton.IsVisible = false;
			orderFormDialog.CancelOrderButton.IsVisible = false;

			orderFormDialog.SaveOrderButton.IsVisible = receivedOrders.All(o => o.IsSaved || o.Status == OrderStatus.Cancelled);
			orderFormDialog.SaveOrderButton.Pressed += OrderForm_SaveButton_Pressed;
			orderFormDialog.BookOrderButton.Pressed += OrderForm_BookButton_Pressed;

			app.ShowDialog(orderFormDialog);
		}

		private void ShowLiveOrderFormAfterChoosingTemplate()
		{
			orderFormDialog = loadLiveOrderFormDialogAfterOtherDialog.EditOrderDialog;
			orderFormDialog.ConfirmButton.IsVisible = false;
			orderFormDialog.RejectButton.IsVisible = false;
			orderFormDialog.CancelOrderButton.IsVisible = false;

			orderFormDialog.SaveOrderButton.IsVisible = true;
			orderFormDialog.SaveOrderButton.Pressed += OrderForm_SaveButton_Pressed;
			orderFormDialog.BookOrderButton.Pressed += OrderForm_BookButton_Pressed;

			orderFormDialog.SaveAsTemplateButton.Text = "Edit Template";
			orderFormDialog.SaveAsTemplateButton.Pressed += OrderForm_EditTemplateButton_Pressed;

			app.ShowDialog(orderFormDialog);
		}

		private void OrderForm_EditTemplateButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			if (!orderFormDialog.IsValid(true, false, false))
			{
				return;
			}

			editTemplateDialog = new EditTemplateDialog((Engine)helpers.Engine);
			editTemplateDialog.CreateNewTemplateButton.Pressed += (s, a) => CreateNewTemplate(orderFormDialog.Order);
			editTemplateDialog.UpdateTemplateButton.Pressed += (s, a) => UpdateOrderTemplate(orderFormDialog.Order);
			editTemplateDialog.DeleteTemplateButton.Pressed += (s, a) => DeleteOrderTemplate();
			editTemplateDialog.BackButton.Pressed += (s, a) => app.ShowDialog(orderFormDialog);

			app.ShowDialog(editTemplateDialog);
		}

		private void CreateNewTemplate(Order order)
		{
			addOrderTemplateDialog = new AddOrderTemplateDialog(helpers, order, userInfo);
			addOrderTemplateDialog.BackButton.Pressed += (s, args) => app.ShowDialog(editTemplateDialog);
			addOrderTemplateDialog.SaveTemplateButton.Pressed += SaveTemplateButton_Pressed;

			app.ShowDialog(addOrderTemplateDialog);
		}

		private void UpdateOrderTemplate(Order order)
		{
			OrderTemplate template = useOrderTemplateDialog.SelectedTemplate;

			addOrderTemplateDialog = new AddOrderTemplateDialog(helpers, order, userInfo);
			addOrderTemplateDialog.Init(template, userInfo);

			addOrderTemplateDialog.BackButton.Pressed += (s, args) => app.ShowDialog(editTemplateDialog);
			addOrderTemplateDialog.SaveTemplateButton.Pressed += (s, args) => SaveUpdatedOrderTemplate(order, addOrderTemplateDialog.TemplateName, addOrderTemplateDialog.SelectedUserGroups.ToArray(), template);

			app.ShowDialog(addOrderTemplateDialog);
		}

		private void SaveUpdatedOrderTemplate(Order order, string templateName, string[] userGroups, OrderTemplate selectedTemplate)
		{
			if (!addOrderTemplateDialog.IsValid) return;

			bool successfullyUpdatedOrderTemplate = helpers.ContractManager.TryEditOrderTemplate(selectedTemplate.Id, templateName, userGroups, order, selectedTemplate.IsPartOfEventTemplate);

			string message = successfullyUpdatedOrderTemplate ? "The template was successfully updated" : "Unable to update the template";

			var messageDialog = new MessageDialog(helpers.Engine, message);

			messageDialog.OkButton.Pressed += (s, args) => helpers.Engine.ExitSuccess("OK");
			app.ShowDialog(messageDialog);
		}

		private void DeleteOrderTemplate()
		{
			string templateName = useOrderTemplateDialog.SelectedTemplate.Name;

			bool successfullyDeletedOrderTemplate = helpers.ContractManager.TryDeleteOrderTemplate(templateName);

			string message = successfullyDeletedOrderTemplate ? "The template was successfully removed" : "Unable to remove the template";

			var messageDialog = new MessageDialog(helpers.Engine, message);

			messageDialog.OkButton.Pressed += (s, args) => helpers.Engine.ExitSuccess("OK");
			app.ShowDialog(messageDialog);
		}

		private void DeleteOrderForm_YesButton_Pressed(object sender, EventArgs e)
		{
			try
			{
				deleteOrderDialog = new DeleteOrderReportDialog(helpers) { Title = "Delete Order" };
				deleteOrderDialog.OkButton.Pressed += DeleteOrderStatus_OkButton_Pressed;

				app.ShowDialog(deleteOrderDialog);

				var tasks = helpers.OrderManager.DeleteOrder(receivedOrder);

				deleteOrderDialog.Finish(tasks);
				app.ShowDialog(deleteOrderDialog);
			}
			catch (Exception)
			{
				ShowMessageDialog(helpers.Engine, $"Unable to delete Order with ID: {loadLiveOrderFormDialog.Orders.FirstOrDefault()?.Id}", "Unable To Delete Order");
			}
		}

		private void DeleteEnclosingEventDialog_YesButton_Pressed(object sender, EventArgs e)
		{
			var messageDialog = new MessageDialog(helpers.Engine);

			if (!helpers.EventManager.DeleteEvent(eventDeletionDialog.JobId))
			{
				messageDialog.Title = "Deleting Event Failed";
				messageDialog.Message = "Failed to delete the enclosing Event.";
				messageDialog.OkButton.Pressed += (send, args) => helpers.Engine.ExitSuccess("Failed to delete the enclosing Event.");
			}
			else
			{
				messageDialog.Title = "Deleting Event Successful";
				messageDialog.Message = "The enclosing Event was successfully deleted.";
				messageDialog.OkButton.Pressed += (send, args) => helpers.Engine.ExitSuccess("The enclosing Event was successfully deleted.");
			}

			app.ShowDialog(messageDialog);
		}

		private void OrderForm_BookButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			helpers.LogMethodStart(nameof(Script), nameof(OrderForm_BookButton_Pressed), out var stopwatch);

			try
			{
				if (!orderFormDialog.IsValid(saveOrder: false, confirmOrder: false, requestEventLock: true))
				{
					helpers.LogMethodCompleted(nameof(Script), nameof(OrderForm_BookButton_Pressed), null, stopwatch);
					return;
				}

				AddOrUpdateOrder(OrderAction.Book);
			}
			catch (Exception exception)
			{
				ShowExceptionDialog(helpers.Engine, exception);
			}

			helpers.LogMethodCompleted(nameof(Script), nameof(OrderForm_BookButton_Pressed), null, stopwatch);
		}

		private void OrderForm_SaveButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			helpers.LogMethodStart(nameof(Script), nameof(OrderForm_SaveButton_Pressed), out var stopwatch);

			try
			{
				if (!orderFormDialog.IsValid(true, false, false))
				{
					helpers.LogMethodCompleted(nameof(Script), nameof(OrderForm_SaveButton_Pressed), null, stopwatch);
					return;
				}

				AddOrUpdateOrder(OrderAction.Save);
			}
			catch (Exception exception)
			{
				ShowExceptionDialog(helpers.Engine, exception);
			}

			helpers.LogMethodCompleted(nameof(Script), nameof(OrderForm_SaveButton_Pressed), null, stopwatch);
		}

		private void SaveAsTemplateButton_Pressed(object sender, YleValueWidgetChangedEventArgs e)
		{
			try
			{
				if (!orderFormDialog.IsValid(true, false, false))
				{
					return;
				}

				addOrderTemplateDialog = new AddOrderTemplateDialog(helpers, orderFormDialog.Order, userInfo);
				addOrderTemplateDialog.BackButton.Pressed += (s, args) => app.ShowDialog(orderFormDialog);
				addOrderTemplateDialog.SaveTemplateButton.Pressed += SaveTemplateButton_Pressed;

				app.ShowDialog(addOrderTemplateDialog);
			}
			catch (Exception exception)
			{
				ShowExceptionDialog(helpers.Engine, exception);
			}
		}

		private void SaveTemplateButton_Pressed(object sender, EventArgs e)
		{
			if (!addOrderTemplateDialog.IsValid) return;

			string templateName = addOrderTemplateDialog.TemplateName;
			string[] userGroups = addOrderTemplateDialog.SelectedUserGroups.ToArray();

			bool successfullyAddedOrderTemplate = helpers.ContractManager.TryAddOrderTemplate(templateName, userGroups, addOrderTemplateDialog.Order, out var templateId);

			string message = successfullyAddedOrderTemplate ? "The template was successfully saved" : "Unable to save the template";

			var messageDialog = new MessageDialog(helpers.Engine, message);

			messageDialog.OkButton.Pressed += (s, args) => helpers.Engine.ExitSuccess("OK");
			app.ShowDialog(messageDialog);
		}

		private void AddOrUpdateOrder(OrderAction orderAction)
		{
			reportDialog = new AddOrUpdateReportDialog(helpers, userInfo.IsMcrUser);
			switch (loadLiveOrderFormDialog.ScriptAction)
			{
				case LiveOrderFormAction.Add:
				case LiveOrderFormAction.FromTemplate:
					reportDialog.Title = "Creating new Order";
					break;
				case LiveOrderFormAction.Edit:
					reportDialog.Title = "Updating Order";
					break;
				case LiveOrderFormAction.Merge:
					reportDialog.Title = "Merging Orders";
					break;
				default:
					// Nothing to do
					break;
			}

			reportDialog.OkButton.Pressed += ReportDialog_OkButton_Pressed;
			reportDialog.RollBackButton.Pressed += ReportDialog_RollbackButton_Pressed;
			app.ShowDialog(reportDialog);

			var tasks = new List<Task>();

			if (scriptAction == LiveOrderFormAction.Merge)
			{
				tasks.AddRange(DeleteMergingOrders().SelectMany(x => x.Tasks));
			}

			var updateResult = orderFormDialog.Finish(orderAction);
			tasks.AddRange(updateResult.Tasks);

			if (tasks.Any())
			{
				reportDialog.Finish(tasks, updateResult.Duration);

				// release the locks if all tasks were successful
				if (tasks.All(t => t.Status == TaskStatus.Ok)) helpers.LockManager.ReleaseLocks();

				app.ShowDialog(reportDialog);
			}
			else ShowMessageDialog(helpers.Engine, "No changes made", "No Changes made");

			helpers.Log(nameof(Script), nameof(AddOrUpdateOrder), $"Order Updated");
		}

		private void ShowProvideReasonForStatusChangeDialog(OrderStatus status)
		{
			try
			{
				provideReasonForStatusChangeDialog = new ProvideReasonForStatusChangeDialog(helpers.Engine, status);
				provideReasonForStatusChangeDialog.OkButton.Pressed += (o, e) => UpdateOrderStatus(status);

				app.ShowDialog(provideReasonForStatusChangeDialog);
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (Exception e)
			{
				ShowExceptionDialog(helpers.Engine, e);
			}
		}

		private void UpdateOrderStatus(OrderStatus status)
		{
			try
			{
				helpers.Engine.ShowProgress($"Setting Order status to {status} ...");

				receivedOrder.ReasonForCancellationOrRejection = provideReasonForStatusChangeDialog?.ReasonForStatusChange;

				receivedOrder.UpdateStatus(helpers, status);

				if (status == OrderStatus.Cancelled && receivedOrder.RecurringSequenceInfo.Recurrence.IsConfigured && orderFormDialog.Order.RecurringSequenceInfo.RecurrenceAction == RecurrenceAction.AllOrdersInSequence)
				{
					receivedOrder.RecurringSequenceInfo.Recurrence.RecurrenceEnding.EndingType = EndingType.SpecificDate;
					receivedOrder.RecurringSequenceInfo.Recurrence.RecurrenceEnding.EndingDateTime = DateTime.Now;
					helpers.OrderManagerElement.UpdateRecurringOrder(receivedOrder, receivedOrder.RecurringSequenceInfo.Id, false);
				}

				string title;
				switch (status)
				{
					case OrderStatus.Rejected:
						title = "Reject Order";
						break;
					case OrderStatus.Confirmed:
						title = "Confirm Order";
						break;
					case OrderStatus.Cancelled:
						title = "Cancel Order";
						break;
					default:
						return;
				}

				ShowMessageDialog(helpers.Engine, "Order has been " + EnumExtensions.GetDescriptionFromEnumValue(status), title);
			}
			catch (ScriptAbortException)
			{
				throw;
			}
			catch (Exception e)
			{
				helpers.Log(nameof(Script), nameof(UpdateOrderStatus), $"Exception occurred: {e}");
				ShowExceptionDialog(helpers.Engine, e);
			}
		}

		private void ReportDialog_OkButton_Pressed(object sender, EventArgs e)
		{
			helpers.LockManager.ReleaseLocks();

			if (reportDialog.TasksWereSuccessful)
			{
				if (scriptAction == LiveOrderFormAction.Merge)
				{
					var eventsWithoutOrders = nonPrimaryMergingOrders.Where(x => !helpers.EventManager.HasOrders(x.Event.Id)).Select(x => x.Event).ToList();
					if (eventsWithoutOrders.Any())
					{
						multipleJobDeletionDialog = new MultipleEventDeletionDialog((Engine)helpers.Engine, eventsWithoutOrders, lockInfos);
						multipleJobDeletionDialog.DeleteSelectedEventsButton.Pressed += DeleteSelectedEventsButton_Pressed;
						app.ShowDialog(multipleJobDeletionDialog);
					}
					else
					{
						helpers.Engine.ExitSuccess(selectedOrderIdForExiting);
					}
				}
				else
				{
					helpers.Engine.ExitSuccess(selectedOrderIdForExiting);
				}
			}
			else if (reportDialog.ShouldRollback)
			{
				RollBackTasks();
			}
			else
			{
				// Exit script when the user chooses to ignore the failed tasks
				helpers.Engine.ExitSuccess(scriptAction == LiveOrderFormAction.Add ? "Unable to create the new Order." : "Unable to update the existing Order.");
			}
		}

		private void DeleteSelectedEventsButton_Pressed(object sender, EventArgs e)
		{
			if (!multipleJobDeletionDialog.GetEventGuidsToDelete().Any())
			{
				helpers.Engine.ExitSuccess(selectedOrderIdForExiting);
			}

			reportDialog = new AddOrUpdateReportDialog(helpers, userInfo.IsMcrUser);

			reportDialog.OkButton.Pressed += MultipleEventDeletionReportDialogOkButton_Pressed;
			reportDialog.RollBackButton.Pressed += ReportDialog_RollbackButton_Pressed;

			app.ShowDialog(reportDialog);

			var tasks = multipleJobDeletionDialog.GetEventGuidsToDelete().Select(x => (Task)new DeleteEventTask(helpers, x)).ToList();

			foreach (var task in tasks) task.Execute();

			reportDialog.Finish(tasks);
			app.ShowDialog(reportDialog);
		}

		private void MultipleEventDeletionReportDialogOkButton_Pressed(object sender, EventArgs e)
		{
			if (reportDialog.TasksWereSuccessful)
			{
				helpers.Engine.ExitSuccess(selectedOrderIdForExiting);
			}
			else if (reportDialog.ShouldRollback)
			{
				RollBackTasks();
			}
			else
			{
				helpers.Engine.ExitSuccess("Unable to delete Events");
			}
		}

		private void ReportDialog_RollbackButton_Pressed(object sender, EventArgs e)
		{
			RollBackTasks();

			ReleaseLocks();
		}

		private void RollBackTasks()
		{
			// Roll back tasks - Only roll back tasks that were successful
			rollbackReportDialog = new RollBackReportDialog(helpers) { Title = scriptAction == LiveOrderFormAction.Edit ? "Roll back Order update" : "Roll back Order creation" };
			rollbackReportDialog.OkButton.Pressed += RollBackReportDialog_OkButton_Pressed;

			app.ShowDialog(rollbackReportDialog);

			// Only roll back tasks that were successful
			var rollbackTasks = reportDialog.UpdateResults.SelectMany(ur => ur.Tasks).Where(t => t.Status == TaskStatus.Ok).Select(t => t.CreateRollbackTask()).Where(t => t != null).Reverse().ToList();

			foreach (var rollbackTask in rollbackTasks)
			{
				if (!rollbackTask.Execute())
				{
					helpers.Log(nameof(Script), nameof(RollBackTasks), "Rolling back " + rollbackTask.Description + "failed: " + rollbackTask.Exception);
					break;
				}
			}

			rollbackReportDialog.Finish(rollbackTasks);
			app.ShowDialog(rollbackReportDialog);
		}

		private void RollBackReportDialog_OkButton_Pressed(object sender, EventArgs e)
		{
			switch (scriptAction)
			{
				case LiveOrderFormAction.Add:
					helpers.Engine.ExitSuccess(rollbackReportDialog.IsSuccessful ? "Successfully rolled back creating the new Order." : "Failed to roll back creating the new Order.");
					break;

				case LiveOrderFormAction.Edit:
					helpers.Engine.ExitSuccess(rollbackReportDialog.IsSuccessful ? "Successfully rolled back updating the existing Order." : "Failed to roll back updating the existing Order.");
					break;

				case LiveOrderFormAction.Delete:
					helpers.Engine.ExitSuccess(rollbackReportDialog.IsSuccessful ? "Successfully rolled back deleting the Order." : "Failed to roll back deleting the Order.");
					break;

				case LiveOrderFormAction.Duplicate:
					helpers.Engine.ExitSuccess(rollbackReportDialog.IsSuccessful ? "Successfully rolled back duplicating the Order." : "Failed to roll back duplicating the Order.");
					break;

				case LiveOrderFormAction.Merge:
					helpers.Engine.ExitSuccess(rollbackReportDialog.IsSuccessful ? "Successfully rolled back merging the Order." : "Failed to roll back merging the Order.");
					break;

				default:
					// Nothing to do
					break;
			}
		}

		private void DeleteOrderStatus_OkButton_Pressed(object sender, EventArgs e)
		{
			// Check if event contains any other Orders, if not, ask user to delete job
			if (!helpers.EventManager.HasOrders(receivedOrder.Event.Id))
			{
				eventDeletionDialog = new EnclosingEventDeletionDialog(helpers.Engine, receivedOrder.Event.Id);

				eventDeletionDialog.YesButton.Pressed += DeleteEnclosingEventDialog_YesButton_Pressed;
				eventDeletionDialog.NoButton.Pressed += (s, args) => helpers.Engine.ExitSuccess("The enclosing Event was not deleted.");

				app.ShowDialog(eventDeletionDialog);
			}
			else
			{
				helpers.Engine.ExitSuccess(deleteOrderDialog.IsSuccessful ? "The Order was successfully deleted." : "The Order was not entirely deleted.");
			}
		}

		private void ShowExceptionDialog(IEngine engine, Exception exception)
		{
			var dialog = new ExceptionDialog((Engine)engine, exception);
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong.");

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		private void ShowMessageDialog(IEngine engine, string message, string title)
		{
			var dialog = new MessageDialog((Engine)engine, message) { Title = title };
			dialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess(message);

			if (app.IsRunning) app.ShowDialog(dialog);
			else app.Run(dialog);
		}

		private IEnumerable<UpdateResult> DeleteMergingOrders()
		{
			helpers.ReportProgress("Deleting merging Orders...");

			var updateResults = new List<UpdateResult>();
			foreach (var mergingOrder in nonPrimaryMergingOrders)
			{
				if (mergingOrder.Id != mergedOrder.Id)
				{
					var tasks = helpers.OrderManager.DeleteOrder(mergingOrder, OrderManager.FlattenServices(mergedOrder.Sources).Select(x => x.Id).ToList());

					updateResults.Add(new UpdateResult
					{
						Tasks = tasks.ToList()
					});
				}
			}

			helpers.ReportProgress("Deleting merging Orders succeeded");

			return updateResults;
		}

		private void ReleaseLocks()
		{
			helpers.LockManager.ReleaseLocks();
		}

		private string SelectIdForExiting(LiteOrder order)
		{
			if (scriptAction == LiveOrderFormAction.Merge) return mergedOrder.Id.ToString();

			// When a new order is created inside an existing event, the event Id will be returned.

			var existingEvent = loadLiveOrderFormDialog.Events.FirstOrDefault();
			if (existingEvent != null) return existingEvent.Id.ToString();

			return order.Id.ToString();
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					helpers.Dispose();

					ReleaseLocks();

					extendLocksTimer.Stop();
					extendLocksTimer.Dispose();
				}
			}

			disposedValue = true;
		}

		~Script()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}
		#endregion
	}
}