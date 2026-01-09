/*
****************************************************************************
*  Copyright (c) 2020,  Skyline Communications NV  All Rights Reserved.    *
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

dd/mm/2020	1.0.0.1		TRE, Skyline	Initial version
****************************************************************************
*/

// Engine.ShowUI();

using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Event;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.Net.ResourceManager.Objects;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;

/// <summary>
/// DataMiner Script Class.
/// </summary>
/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script : IDisposable
{
	private InteractiveController app;

	private Helpers helpers;

	private LaunchDialog dialog;

	private Engine engine;

	private string CustomerUiScriptName;

	private Element orderBookingManagerElement;
	private bool disposedValue;

	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLScripting process.</param>
	public void Run(Engine engine)
	{
		try
		{
			this.engine = engine;
			engine.SetFlag(RunTimeFlags.NoKeyCaching);
			engine.SetFlag(RunTimeFlags.NoCheckingSets);
			engine.Timeout = TimeSpan.FromHours(10);
			RunSafe(engine);
		}
		catch (ScriptAbortException)
		{
			throw;
		}
		catch (Exception e)
		{
			engine.Log("Run|Something went wrong: " + e);
			ShowExceptionDialog(engine, e);
		}
	}

	private void RunSafe(Engine engine)
	{
		// Init CustomerUiScriptName
		CustomerUiScriptName = engine.GetScriptParam("CustomerUiScriptName").Value;

		// Find Order Booking Manager
		orderBookingManagerElement = engine.FindElementsByName("Order Booking Manager").FirstOrDefault();
		if (orderBookingManagerElement == null)
		{
			engine.ExitFail("Unable to find Order Booking Manager");
		}

		// Launch Dialog
		app = new InteractiveController(engine);
		helpers = new Helpers(engine, Scripts.CustomerUiLauncher);
		dialog = new LaunchDialog(engine, helpers.OrderManager.GetAllFutureAndOngoingVideoReservations(), helpers.EventManager.GetAllEvents());

		dialog.ViewButton.Pressed += ViewButton_Pressed;
		dialog.AddButton.Pressed += AddButton_Pressed;
		dialog.AddFromTemplateButton.Pressed += AddFromTemplateButton_Pressed;
		dialog.AddToEventButton.Pressed += AddToEventButton_Pressed;
		dialog.EditButton.Pressed += EditButton_Pressed;
		dialog.DuplicateButton.Pressed += DuplicateButton_Pressed;

		app.Run(dialog);
	}

	private void EditButton_Pressed(object sender, EventArgs e)
	{
		ReservationInstance selectedOrder = dialog.SelectedOrder;
		Guid guid = Guid.Empty;
		if (selectedOrder != null) guid = selectedOrder.ID;

		SubScriptOptions subscriptInfo = engine.PrepareSubScript(CustomerUiScriptName);

		subscriptInfo.SelectScriptParam("JobID", "-1");
		subscriptInfo.SelectScriptParam("BookingManagerID", String.Format("{0}/{1}", orderBookingManagerElement.DmaId, orderBookingManagerElement.ElementId));
		subscriptInfo.SelectScriptParam("BookingID", guid.ToString());
		subscriptInfo.SelectScriptParam("Action", "Edit");
		subscriptInfo.SelectScriptParam("OrderType", "Video");

		subscriptInfo.Synchronous = true;
		subscriptInfo.StartScript();

		RefreshOrders();
		RefreshEvents();
	}


	private void DuplicateButton_Pressed(object sender, EventArgs e)
	{
		ReservationInstance selectedOrder = dialog.SelectedOrder;
		Guid guid = Guid.Empty;
		if (selectedOrder != null) guid = selectedOrder.ID;

		SubScriptOptions subscriptInfo = engine.PrepareSubScript(CustomerUiScriptName);

		subscriptInfo.SelectScriptParam("JobID", "-1");
		subscriptInfo.SelectScriptParam("BookingManagerID", String.Format("{0}/{1}", orderBookingManagerElement.DmaId, orderBookingManagerElement.ElementId));
		subscriptInfo.SelectScriptParam("BookingID", guid.ToString());
		subscriptInfo.SelectScriptParam("Action", "Duplicate");
		subscriptInfo.SelectScriptParam("OrderType", "Video");

		subscriptInfo.Synchronous = true;
		subscriptInfo.StartScript();

		RefreshOrders();
		RefreshEvents();
	}

	private void ViewButton_Pressed(object sender, EventArgs e)
	{
		ReservationInstance selectedOrder = dialog.SelectedOrder;
		Guid guid = Guid.Empty;
		if (selectedOrder != null) guid = selectedOrder.ID;

		SubScriptOptions subscriptInfo = engine.PrepareSubScript(CustomerUiScriptName);

		subscriptInfo.SelectScriptParam("JobID", "-1");
		subscriptInfo.SelectScriptParam("BookingManagerID", String.Format("{0}/{1}", orderBookingManagerElement.DmaId, orderBookingManagerElement.ElementId));
		subscriptInfo.SelectScriptParam("BookingID", guid.ToString());
		subscriptInfo.SelectScriptParam("Action", "View");
		subscriptInfo.SelectScriptParam("OrderType", "Video");

		subscriptInfo.Synchronous = true;
		subscriptInfo.StartScript();
	}


	private void AddButton_Pressed(object sender, EventArgs e)
	{
		SubScriptOptions subscriptInfo = engine.PrepareSubScript(CustomerUiScriptName);

		subscriptInfo.SelectScriptParam("JobID", "-1");
		subscriptInfo.SelectScriptParam("BookingManagerID", String.Format("{0}/{1}", orderBookingManagerElement.DmaId, orderBookingManagerElement.ElementId));
		subscriptInfo.SelectScriptParam("BookingID", "-1");
		subscriptInfo.SelectScriptParam("Action", "Add new");
		subscriptInfo.SelectScriptParam("OrderType", "Video");

		subscriptInfo.Synchronous = true;
		subscriptInfo.StartScript();

		RefreshOrders();
		RefreshEvents();
	}

	private void AddFromTemplateButton_Pressed(object sender, EventArgs e)
	{
		SubScriptOptions subscriptInfo = engine.PrepareSubScript(CustomerUiScriptName);

		subscriptInfo.SelectScriptParam("JobID", "-1");
		subscriptInfo.SelectScriptParam("BookingManagerID", String.Format("{0}/{1}", orderBookingManagerElement.DmaId, orderBookingManagerElement.ElementId));
		subscriptInfo.SelectScriptParam("BookingID", "-1");
		subscriptInfo.SelectScriptParam("Action", "From template");
		subscriptInfo.SelectScriptParam("OrderType", "Video");

		subscriptInfo.Synchronous = true;
		subscriptInfo.StartScript();

		RefreshOrders();
		RefreshEvents();
	}

	private void AddToEventButton_Pressed(object sender, EventArgs e)
	{
		Event selectedEvent = dialog.SelectedEvent;
		Guid guid = Guid.Empty;
		if (selectedEvent != null) guid = selectedEvent.Id;

		SubScriptOptions subscriptInfo = engine.PrepareSubScript(CustomerUiScriptName);

		subscriptInfo.SelectScriptParam("JobID", guid.ToString());
		subscriptInfo.SelectScriptParam("BookingManagerID", String.Format("{0}/{1}", orderBookingManagerElement.DmaId, orderBookingManagerElement.ElementId));
		subscriptInfo.SelectScriptParam("BookingID", "-1");
		subscriptInfo.SelectScriptParam("Action", "Add");
		subscriptInfo.SelectScriptParam("OrderType", "Video");

		subscriptInfo.Synchronous = true;
		subscriptInfo.StartScript();

		RefreshOrders();
		RefreshEvents();
	}

	private void RefreshOrders()
	{
		dialog.UpdateOrders(helpers.OrderManager.GetAllFutureAndOngoingVideoReservations());
	}

	private void RefreshEvents()
	{
		dialog.UpdateEvents(helpers.EventManager.GetAllEvents());
	}

	private void ShowExceptionDialog(Engine engine, Exception exception)
	{
		ExceptionDialog exceptionDialog = new ExceptionDialog(engine, exception);
		exceptionDialog.OkButton.Pressed += (sender, args) => engine.ExitSuccess("Something went wrong during the creation of the new event.");
		exceptionDialog.Show();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				helpers.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

public class LaunchDialog : Dialog
{
	private IEnumerable<ReservationInstance> orders;
	private IEnumerable<Event> events;

	public LaunchDialog(Engine engine, IEnumerable<ReservationInstance> orders, IEnumerable<Event> events)
		: base(engine)
	{
		this.orders = orders;
		this.events = events;

		Title = "Customer UI Launcher";

		OrderDropdown = new DropDown(orders.Select(x => x.Name)) { Width = 500, IsDisplayFilterShown = true };
		EventDropDown = new DropDown(events.Select(x => x.Name)) { Width = 500, IsDisplayFilterShown = true };
		AddButton = new Button("Add");
		AddFromTemplateButton = new Button("Add From Template");
		AddToEventButton = new Button("Add to Event");
		EditButton = new Button("Edit");
		DuplicateButton = new Button("Duplicate");
		ViewButton = new Button("View");

		GenerateUI();
	}

	public void UpdateOrders(IEnumerable<ReservationInstance> orders)
	{
		this.orders = orders;
		OrderDropdown.Options = orders.Select(x => x.Name);
	}

	public void UpdateEvents(IEnumerable<Event> events)
	{
		this.events = events;
		EventDropDown.Options = events.Select(x => x.Name);
	}

	public ReservationInstance SelectedOrder
	{
		get
		{
			return orders.FirstOrDefault(x => x.Name.Equals(OrderDropdown.Selected));
		}
	}

	public Event SelectedEvent
	{
		get
		{
			return events.FirstOrDefault(x => x.Name.Equals(EventDropDown.Selected));
		}
	}

	public DropDown EventDropDown
	{
		get;
		private set;
	}

	public DropDown OrderDropdown
	{
		get;
		private set;
	}

	public Button AddButton
	{
		get;
		private set;
	}

	public Button AddFromTemplateButton
	{
		get;
		private set;
	}

	public Button AddToEventButton
	{
		get;
		private set;
	}

	public Button EditButton
	{
		get;
		private set;
	}

	public Button DuplicateButton
	{
		get;
		private set;
	}

	public Button ViewButton
	{
		get;
		private set;
	}

	private void GenerateUI()
	{
		int row = -1;
		AddWidget(new Label("Create new Order"), ++row, 0);
		AddWidget(AddButton, row, 1);
		AddWidget(AddFromTemplateButton, ++row, 1);

		AddWidget(EventDropDown, ++row, 0);
		AddWidget(AddToEventButton, row, 1);

		AddWidget(OrderDropdown, ++row, 0);
		AddWidget(ViewButton, row, 1);
		AddWidget(EditButton, ++row, 1);
		AddWidget(DuplicateButton, row + 1, 1);
	}
}