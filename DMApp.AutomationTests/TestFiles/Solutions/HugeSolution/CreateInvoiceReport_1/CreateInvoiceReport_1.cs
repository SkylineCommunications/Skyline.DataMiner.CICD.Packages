/*
****************************************************************************
*  Copyright (c) 2022,  Skyline Communications NV  All Rights Reserved.    *
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

dd/mm/2022	1.0.0.1		XXX, Skyline	Initial version
****************************************************************************
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Configuration;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Reports;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.ReportTasks;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.UI.Reports;
using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;
using Task = Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Tasks.Task;

/// <summary>
/// DataMiner Script Class.
/// </summary>
public class Script : IDisposable
{
	private InvoiceReportDialog invoiceReportDialog;
	private LoadInvoiceReportDialog loadInvoiceReportDialog;

	private Helpers helpers;
	private InteractiveController app;

	private bool disposedValue;

	/// <summary>
	/// The Script entry point.
	/// </summary>
	/// <param name="engine">Link with SLAutomation process.</param>
	public void Run(Engine engine)
	{
		try
		{
			//engine.ShowUI()
			helpers = new Helpers(engine, Scripts.CreateInvoiceReport);

			app = new InteractiveController(engine);

			loadInvoiceReportDialog = new LoadInvoiceReportDialog(helpers);

			if (loadInvoiceReportDialog.Execute()) ShowCreateInvoiceReportDialog();
			else app.Run(invoiceReportDialog);
		}
		catch (ScriptAbortException)
		{
			helpers?.Log(nameof(Script), nameof(Run), "STOP SCRIPT", "Create Invoice Report");
			Dispose();
		}
		catch (InteractiveUserDetachedException)
		{
			helpers?.Log(nameof(Script), nameof(Run), "STOP SCRIPT", "Create Invoice Report");
			Dispose();
		}
		catch (Exception e)
		{
			helpers?.Log(nameof(Script), nameof(Run), "STOP SCRIPT", "Create Invoice Report");
			engine.Log("Run|Something went wrong: " + e);

			Dispose();
		}
	}

	private void ShowCreateInvoiceReportDialog()
	{
		invoiceReportDialog = loadInvoiceReportDialog.InvoiceReportDialog;
		invoiceReportDialog.CreateInvoiceReportButton.Pressed += CreateInvoiceReportButton_Pressed;

		if (app.IsRunning) app.ShowDialog(invoiceReportDialog);
		else app.Run(invoiceReportDialog);
	}

	private void CreateInvoiceReportButton_Pressed(object sender, EventArgs e)
	{
		string selectedCompany = invoiceReportDialog.SelectedCompany;
		DateTime selectedStartTime = invoiceReportDialog.SelectedStartTime;
		DateTime selectedEndTime = invoiceReportDialog.SelectedEndTime;

		try
		{
			var reportDialog = new AddOrUpdateReportDialog(helpers);
			reportDialog.OkButton.Pressed += (o, args) => { helpers.Engine.ExitSuccess($"Successfully Generated Invoice Report for company {selectedCompany}"); };
			app.ShowDialog(reportDialog);

			var tasks = new List<Task>();
			var generateInvoiceReportExcelTask = new GenerateInvoiceReportExcelTask(helpers, selectedStartTime, selectedEndTime, selectedCompany);
			tasks.Add(generateInvoiceReportExcelTask);

			generateInvoiceReportExcelTask.Execute();

			if (tasks.Any())
			{
				reportDialog.Finish(tasks);
			}
		}
		catch (Exception ex)
		{
			helpers.Log(nameof(Script), nameof(CreateInvoiceReportButton_Pressed), $"Something went wrong during creation of the invoice report for company {selectedCompany}: " + ex);
		}
	}

	#region IDisposable Support
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue && disposing)
		{	
			helpers?.Dispose();	
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