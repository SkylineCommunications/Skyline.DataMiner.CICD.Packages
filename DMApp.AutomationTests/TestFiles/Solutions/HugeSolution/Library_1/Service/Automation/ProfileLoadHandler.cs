namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading;
	using Newtonsoft.Json;
	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.DataMinerInterface;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions.Automation;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service.Automation.Sets;
	using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
	using Skyline.DataMiner.Library.Profile;
	using Skyline.DataMiner.Library.Solutions.SRM;
	using Skyline.DataMiner.Library.Solutions.SRM.LifecycleServiceOrchestration;
	using Skyline.DataMiner.Library.Solutions.SRM.Logging.Orchestration;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Profiles;
	using Skyline.DataMiner.Net.ResourceManager.Objects;

	public class ProfileLoadHandler : HelpedObject
    {
        private ProfileParameterEntryHelper profileParameterHelper;
        private SetsToExecuteOnDevice setsToExecuteOnDevice;
        private SrmResourceConfigurationInfo srmResourceConfigurationInfo;
        private GetProtocolInfoResponseMessage protocolInfo;
        private Dictionary<int, string> tableKeyMapping = new Dictionary<int, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileLoadHandler" /> class.
        /// </summary>
        /// <param name="helpers">The engine reference.</param>
        private ProfileLoadHandler(Helpers helpers) : base (helpers)
        {
            Initialize();
        }

        public static void ApplyConfiguration(Helpers helpers)
		{
            var profileLoadHandler = new ProfileLoadHandler(helpers);

            profileLoadHandler.ApplyConfiguration();
		}

        private void ApplyConfiguration()
        {
            switch (srmResourceConfigurationInfo.ProfileAction)
            {
                case "": // to support the Profile Load Script Tester element
                case "APPLY":
                    ApplyProfileConfigurations();
                    break;

                case "STOP":
                    break;

                default:
                    throw new ArgumentException($"Unknown ProfileAction script parameter: '{srmResourceConfigurationInfo.ProfileAction}'");
            }
        }

        /// <summary>
        ///     Retrieves the <see cref="GetProtocolInfoResponseMessage" /> for the given protocol and version.
        /// </summary>
        /// <param name="protocolName">The name of the protocol.</param>
        /// <param name="protocolVersion">The version of the protocol.</param>
        /// <returns>An <see cref="GetProtocolInfoResponseMessage" /> for the protocol.</returns>
        private GetProtocolInfoResponseMessage GetProtocolInfo(string protocolName, string protocolVersion)
        {
            var getProtocolRequest = new GetProtocolMessage(protocolName, protocolVersion);

            if (this.helpers.Engine.SendSLNetSingleResponseMessage(getProtocolRequest) is GetProtocolInfoResponseMessage protocolInfoResponse)
            {
                return protocolInfoResponse;
            }

            Log(nameof(GetProtocolInfo), $"GetProtocolMessage | Protocol {protocolName} {protocolVersion} not found");

            return new GetProtocolInfoResponseMessage();
        }

		/// <summary>
		/// Sets the value to the resource parameter.
		/// </summary>
		/// <param name="setToExecute"></param>
		/// <param name="resourceElement"></param>
		/// <param name="viaSlNet">Optional; Indicates if a set needs to be done through SLNET, <see langword="false" /> if none.</param>
		/// <returns>The indication of the set parameter was successful.</returns>
		private bool SetParameter(ISetToExecute setToExecute, Element resourceElement, bool viaSlNet = false)
        {
            if (setToExecute.ValueToSet is null)
            {
                Log(nameof(SetParameter), $"Setting parameter {setToExecute.Description} skipped because it's value is null");

                return false;
            }

            if (!CheckParameterConfiguration(setToExecute))
            {
                Log(nameof(SetParameter), $"Setting parameter {setToExecute.Description} skipped because it's value is an empty string or not a valid double");

                return false;
            }

            int tablePid = this.GetTablePidFromColumn(setToExecute.ProtocolReadParameterId);

            bool parameterIsPartOfTable = tablePid != 0;

            if (parameterIsPartOfTable)
            {
                string primaryKey = this.GetKey(resourceElement, tablePid);
                return this.SetTableParam(setToExecute, resourceElement, primaryKey, viaSlNet);
            }
			else
			{
                return this.SetSingleParameter(setToExecute, resourceElement, viaSlNet);
            } 
        }

        /// <summary>
        /// Sets the single parameter through SLNET.
        /// </summary>
        /// <param name="resourceElement">The element of the resource.</param>
        /// <param name="paramId">The Parameter ID.</param>
        /// <param name="value">The value that needs to be set.</param>
        private void SetSingleParamViaSlNet(Element resourceElement, int paramId, string value)
        {
            try
            {
                string raw = resourceElement.GetRawValue(paramId, value);
                string rawValue = string.IsNullOrWhiteSpace(raw) ? value : raw;
                var spm = new SetParameterMessage { DataMinerID = resourceElement.DmaId, ElId = resourceElement.ElementId, ParameterId = paramId, Value = new Net.Messages.ParameterValue(rawValue) };
                Engine.SLNet.SendMessage(spm);

                Log(nameof(SetSingleParamViaSlNet), $"Setting parameter via SLNet succeeded for parameter id: {paramId} on element: {resourceElement.ElementName} with value: {value}");
            }
            catch (Exception e)
            {
                Log(nameof(SetSingleParamViaSlNet), $"SET failed on '{resourceElement.ElementName}': failed to set the value '{value}' to parameter {paramId}. {e.Message}");
                throw;
            }
        }

        /// <summary>
        ///     Sets the table parameter through SLNET.
        /// </summary>
        /// <param name="resourceElement">The element of the resource.</param>
        /// <param name="paramId">The Parameter ID.</param>
        /// <param name="primaryKey">The primary key of the table row.</param>
        /// <param name="value">The value that needs to be set.</param>
        private void SetTableParamViaSlNet(Element resourceElement, int paramId, string primaryKey, string value)
        {
            try
            {
                string raw = resourceElement.GetRawValue(paramId, value);
                string rawValue = string.IsNullOrWhiteSpace(raw) ? value : raw;
                var spm = new SetParameterMessage { DataMinerID = resourceElement.DmaId, ElId = resourceElement.ElementId, ParameterId = paramId, TableIndex = primaryKey, TableIndexPreference = SetParameterTableIndexPreference.ByPrimaryKey, Value = new Net.Messages.ParameterValue(rawValue) };
                Engine.SLNet.SendMessage(spm);

                Log( nameof(SetTableParamViaSlNet), $"Setting table parameter via SLNet succeeded for parameter id: {paramId} on element: {resourceElement.ElementName} with value: {value}");
            }
            catch (Exception e)
            {
                Log(nameof(SetTableParamViaSlNet), $"SET failed on '{resourceElement.ElementName}': failed to set the value '{value}' to parameter {paramId}. {e.Message}");
                throw;
            }
        }

		/// <summary>
		///     Verify the set of the profile parameter.
		/// </summary>
		/// <param name="setToExecute">The element of the resource.</param>
		/// <param name="resourceElement"></param>
		/// <param name="retries">The number of retries.</param>
		/// <returns>The indication of the validation.</returns>
		private bool VerifySetOnParameter(ISetToExecute setToExecute, Element resourceElement)
        {
            int tablePid = this.GetTablePidFromColumn(setToExecute.ProtocolReadParameterId);
            if (tablePid == 0)
            {
                return this.ValidateProfileParameter(setToExecute, resourceElement);
            }

            string tableKey = this.GetKey(resourceElement, tablePid);
            return this.ValidateProfileParameter(setToExecute, resourceElement, tableKey);
        }

        /// <summary>
        ///     Checks if the parameterConfiguration value is valid.
        /// </summary>
        /// <param name="setToExecute">The current resource parameter configuration.</param>
        /// <returns>The indication if the value is valid.</returns>
        private bool CheckParameterConfiguration(ISetToExecute setToExecute)
        {
            if (setToExecute.ValueToSet is null)
            {
                return false;
            }

            if (setToExecute.ValueToSet is double doubleValue)
            {
                if (double.IsNaN(doubleValue))
                {
                    return false;
                }
            }
            else if(setToExecute.ValueToSet is string stringValue)
            {
                if (string.IsNullOrEmpty(stringValue))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Retrieves the Table PID for a certain column PID.
        /// </summary>
        /// <param name="columnPid">The PID of the column parameter.</param>
        /// <returns>The PID of the table. Returns 0 when the parameter is not part of a table.</returns>
        private int GetTablePidFromColumn(int columnPid)
        {
            ParameterInfo parameterInfo = this.protocolInfo.Parameters.FirstOrDefault(x => x.ID == columnPid);
            if (parameterInfo != null && parameterInfo.IsTableColumn)
            {
                Log(nameof(VerifySetOnParameter), $"Found parent table: {parameterInfo.ParentTablePid} for column {columnPid}");

                return parameterInfo.ParentTablePid;
            }

            return 0;
        }

        /// <summary>
        ///     Retrieves the primary key for the given table PID.
        /// </summary>
        /// <param name="resourceElement">The element of the resource.</param>
        /// <param name="tablePid">The current table PID.</param>
        /// <returns>The primary key for the table PID.</returns>
        private string GetKey(Element resourceElement, int tablePid)
        {
            if (this.tableKeyMapping.Keys.Contains(tablePid))
            {
                return this.tableKeyMapping[tablePid];
            }

            return resourceElement.GetTablePrimaryKeys(tablePid).FirstOrDefault();
        }

        /// <summary>
        ///     Checks if the protocolInfo field is up to date with the protocol information of the resource element.
        /// </summary>
        /// <param name="resourceElement">The element of the resource.</param>
        private void GetResourceElementProtocolInfo(Element resourceElement)
        {
            try
            {
                if (this.protocolInfo == null || !this.protocolInfo.Version.Equals(resourceElement.ProtocolVersion) || this.protocolInfo.Name.Equals(resourceElement.ProtocolName))
                {
                    this.protocolInfo = this.GetProtocolInfo(resourceElement.ProtocolName, resourceElement.ProtocolVersion);
                }
            }
            catch (Exception e)
            {
                Log(nameof(GetResourceElementProtocolInfo), $"Failed to retrieve protocol info. {e.Message}");
                throw;
            }
        }

        private bool RequestMediationValue(SrmParameterConfiguration parameterConfiguration, object paramValue, ProtocolParameterReference entry, out object value)
        {
            var request = new MediateProfileToDeviceRequest { Value = DataMiner.MediationSnippets.ParameterValue.CreateValue(paramValue), DeviceID = new ElementID(parameterConfiguration.ResourceElement.DmaId, parameterConfiguration.ResourceElement.ElementId), MeddiationSnippetId = entry.MediationSnippetID };
            var response = this.helpers.Engine.SendSLNetSingleResponseMessage(request) as MediateProfileToDeviceResponse;
            value = paramValue;
            if (response != null && response.ParameterSets != null && response.Error == null)
            {
                Log(nameof(RequestMediationValue), $"Found Mediated Parameter|{parameterConfiguration.ProfileParameterName}|{paramValue}|{response.Value}");

                value = response.Value.GetValue();
                return true;
            }

            if (response != null && response.Error != null)
            {
                throw new SrmConfigurationException($"Error while converting parameter|{parameterConfiguration.ProfileParameterName}|{response.Error}");
            }

            return false;
        }

        /// <summary>
        ///     Retrieves the Mediation value.
        /// </summary>
        /// <param name="parameterConfiguration">The current resource parameter configuration.</param>
        /// <param name="parameterReference">The current protocol resource configuration.</param>
        /// <returns>The value converted by the mediation snippet. If there is no snippet the original value is returned.</returns>
        private object RetrieveMediationValue(SrmParameterConfiguration parameterConfiguration, ProtocolParameterReference parameterReference)
        {
            try
            {
                object paramValue = parameterConfiguration.Value.GetValue();
                if (this.protocolInfo == null)
                {
                    Log(nameof(RetrieveMediationValue), "Unable to retrieve protocol info.");
                    return paramValue;
                }

                var profileParameter = SrmManagers.ProfileManager.GetParametersWithFilter(ParameterExposers.Name.Equal(parameterConfiguration.ProfileParameterName)).FirstOrDefault();
                if (profileParameter == null)
                {
                    Log(nameof(RetrieveMediationValue), "Unable to retrieve profile parameter.");

                    return paramValue;
                }

                if (parameterConfiguration.ResourceElement.ProtocolVersion.Equals("Production", StringComparison.InvariantCultureIgnoreCase))
                {
                    Log(nameof(RetrieveMediationValue), $"Map Production version to version {this.protocolInfo.Version}.");
                }

                ProtocolParameterReference entry;
                try
                {
                    entry = profileParameter.ResolveReference(new ProtocolID(parameterConfiguration.ResourceElement.ProtocolName, parameterConfiguration.ResourceElement.ProtocolVersion));
                }
                catch (Exception)
                {
                    Log(nameof(RetrieveMediationValue), $"No Mediation found for parameter {parameterConfiguration.ProfileParameterName} {parameterConfiguration.ResourceElement.ProtocolName} {this.protocolInfo.Version}. SRM parameter value will be returned.");

                    return paramValue;
                }

                if (entry.ParameterId != parameterReference.ParameterId)
                {
                    Log(nameof(RetrieveMediationValue), $"Guessed the wrong entry for {profileParameter} {parameterReference.ParameterId} {parameterConfiguration.ResourceElement.ElementName} {entry}");
                }

                if (this.RequestMediationValue(parameterConfiguration, paramValue, entry, out object value))
                {
                    return value;
                }

                return paramValue;
            }
            catch (Exception e)
            {
                Log(nameof(RetrieveMediationValue), $"Something went wrong: {e}");
                throw;
            }
        }

		/// <summary>
		///     Sets the value to the single resource parameter.
		/// </summary>
		/// <param name="setToExecute">The current resource parameter configuration.</param>
		/// <param name="resourceElement"></param>
		/// <param name="viaSlNet">Optional; Indicates if a set needs to be done through SLNET, <see langword="false" /> if none.</param>
		/// <returns>The indication of the set parameter was successful.</returns>
		private bool SetSingleParameter(ISetToExecute setToExecute, Element resourceElement, bool viaSlNet = false)
        {
            string currentParameterValue;

            var deviceValue = resourceElement.GetParameter(setToExecute.ProtocolReadParameterId);

            if (deviceValue != null && setToExecute is SetToExecuteBasedOnProfileParameter setToExecuteBasedOnProfileParameter)
			{
                currentParameterValue = setToExecuteBasedOnProfileParameter.Mediator.ConvertDeviceToProfile(null, new DataMiner.MediationSnippets.ParameterSet(setToExecute.ProtocolReadParameterId, DataMiner.MediationSnippets.ParameterValue.CreateValue(deviceValue))).GetValue().ToString();
			}
			else
			{
                currentParameterValue = Convert.ToString(deviceValue);
            }

            var expectedParameterValue = Convert.ToString(setToExecute.ValueToSet);
            if (currentParameterValue == expectedParameterValue)
            {
                Log(nameof(SetSingleParameter), $"Not setting parameter {setToExecute.Description} on {resourceElement.ElementName} because it's value is already set to {expectedParameterValue}");
                return false;
            }

			int writeParameterId = resourceElement.GetWriteParameterIDFromRead(setToExecute.ProtocolReadParameterId);

			Log(nameof(SetSingleParameter), $"Read parameter ID {setToExecute.ProtocolReadParameterId} has write parameter ID {writeParameterId}");

			if (writeParameterId == -1)
			{
				Log(nameof(SetSingleParameter), $"Unable to execute set as no valid write parameter ID was found");
				return false;
			}

			try
			{
				if (viaSlNet)
                {
                    this.SetSingleParamViaSlNet(resourceElement, setToExecute.Description, writeParameterId, expectedParameterValue);
                }
                else
                {

					Log(nameof(SetSingleParameter), $"Setting {setToExecute.Description} with value {setToExecute.ValueToSet} on parameter {writeParameterId}");

					resourceElement.SetParameter(writeParameterId, setToExecute.ValueToSet);
                }

                return true;
            }
            catch (Exception e)
            {
                Log(nameof(SetSingleParameter), $"Exception occurred while setting {setToExecute.Description} with value {setToExecute.ValueToSet} on parameter {writeParameterId}: {e}");
                return false;
            }
        }

        /// <summary>
        ///     Sets the single parameter through SLNET.
        /// </summary>
        /// <param name="resourceElement">
        ///     The element of the resource.
        /// </param>
        /// <param name="parameterName">
        ///     The parameter Name.
        /// </param>
        /// <param name="paramId">
        ///     The Parameter ID.
        /// </param>
        /// <param name="value">
        ///     The value that needs to be set.
        /// </param>
        private void SetSingleParamViaSlNet(Element resourceElement, string parameterName, int paramId, string value)
        {
            string raw = resourceElement.GetRawValue(paramId, value);
            string rawValue = string.IsNullOrWhiteSpace(raw) ? value : raw;
            var spm = new SetParameterMessage { DataMinerID = resourceElement.DmaId, ElId = resourceElement.ElementId, ParameterId = paramId, Value = new Net.Messages.ParameterValue(rawValue) };
            Engine.SLNet.SendMessage(spm);
            this.profileParameterHelper.LogSuccessResult(resourceElement.ElementName, parameterName, value);

            Log(nameof(SetSingleParamViaSlNet), $"SET succeeded on '{resourceElement.ElementName}': succeeded to set the value '{value}'.");
        }

		/// <summary>
		///     Sets the value to the table resource parameter.
		/// </summary>
		/// <param name="setToExecute">The current resource parameter configuration.</param>
		/// <param name="resourceElement"></param>
		/// <param name="primaryKey">The primary key of the table row.</param>
		/// <param name="viaSLNet">Optional; Indicates if a set needs to be done through SLNET, <see langword="false" /> if none.</param>
		/// <returns>The indication of the set parameter was successful.</returns>
		private bool SetTableParam(ISetToExecute setToExecute, Element resourceElement, string primaryKey, bool viaSLNet = false)
        {
            if (string.IsNullOrWhiteSpace(primaryKey))
            {
                Log(nameof(SetTableParam), $"Parameter {setToExecute.Description} doesn't have a valid value primary key.");

                return false;
            }

            string currentParameterValue;

            var deviceValue = resourceElement.GetParameter(setToExecute.ProtocolReadParameterId);

            if (setToExecute is SetToExecuteBasedOnProfileParameter setToExecuteBasedOnProfileParameter)
            {
                currentParameterValue = setToExecuteBasedOnProfileParameter.Mediator.ConvertDeviceToProfile(null, new DataMiner.MediationSnippets.ParameterSet(setToExecute.ProtocolReadParameterId, DataMiner.MediationSnippets.ParameterValue.CreateValue(deviceValue))).GetStringValue();
            }
            else
            {
                currentParameterValue = Convert.ToString(deviceValue);
            }

            var expectedParameterValue = Convert.ToString(setToExecute.ValueToSet);
            if (currentParameterValue == expectedParameterValue)
            {
                Log(nameof(SetTableParam), $"Not setting parameter {setToExecute.Description} on {resourceElement.ElementName} because it's value is already set to {expectedParameterValue}.");
                return false;
            }

            try
            {
                if (viaSLNet)
                {
                    this.SetTableParamViaSlNet(resourceElement, setToExecute.Description, resourceElement.GetWriteParameterIDFromRead(setToExecute.ProtocolReadParameterId), primaryKey, expectedParameterValue);
                }
                else
                {
                    resourceElement.SetParameter(resourceElement.GetWriteParameterIDFromRead(setToExecute.ProtocolReadParameterId), primaryKey, setToExecute.ValueToSet);
                    this.profileParameterHelper.LogSuccessResult(resourceElement.ElementName, setToExecute.Description, setToExecute.ValueToSet.ToString());

                    Log(nameof(SetTableParam), $"Profile parameter: {setToExecute.Description} was set correctly with following value: {setToExecute.ValueToSet} on element: {resourceElement.ElementName}");
                }

                return true;
            }
            catch (Exception e)
            {
                Log(nameof(SetTableParam), $"SET failed on '{resourceElement.ElementName}': failed to set the value '{setToExecute.ValueToSet}'. {e.Message}");
                return false;
            }
        }

        /// <summary>
        ///     Sets the table parameter through SLNET.
        /// </summary>
        /// <param name="resourceElement">
        ///     The element of the resource.
        /// </param>
        /// <param name="parameterName">
        ///     The parameter Name.
        /// </param>
        /// <param name="paramId">
        ///     The Parameter ID.
        /// </param>
        /// <param name="primaryKey">
        ///     The primary key of the table row.
        /// </param>
        /// <param name="value">
        ///     The value that needs to be set.
        /// </param>
        private void SetTableParamViaSlNet(Element resourceElement, string parameterName, int paramId, string primaryKey, string value)
        {
            string raw = resourceElement.GetRawValue(paramId, value);
            string rawValue = string.IsNullOrWhiteSpace(raw) ? value : raw;
            var spm = new SetParameterMessage { DataMinerID = resourceElement.DmaId, ElId = resourceElement.ElementId, ParameterId = paramId, TableIndex = primaryKey, TableIndexPreference = SetParameterTableIndexPreference.ByPrimaryKey, Value = new Net.Messages.ParameterValue(rawValue) };
            Engine.SLNet.SendMessage(spm);

            Log(nameof(SetTableParamViaSlNet), $"Profile parameter: {parameterName} was set correctly with following value: {value} on element: {resourceElement.ElementName}");
        }

		/// <summary>
		///     Validates the set of the parameter configuration.
		/// </summary>
		/// <param name="setToExecute">The current resource parameter configuration.</param>
		/// <param name="resourceElement"></param>
		/// <param name="retries">The number of retries.</param>
		/// <param name="primaryKey">
		///     Optional; Indicates if the validation needs to be done on a table row, <see langword="null" />
		///     if none.
		/// </param>
		/// <returns>The indication of the set parameter was successful.</returns>
		private bool ValidateProfileParameter(ISetToExecute setToExecute, Element resourceElement, string primaryKey = null)
        {
            if (!this.CheckParameterConfiguration(setToExecute))
            {
                Log(nameof(ValidateProfileParameter), $"Skip verifying set on parameter {setToExecute.Description} because it's value is an empty string or not a valid double");
                return false;
            }

            if (this.ValidateProfileParameterWithMaxRetries(resourceElement, setToExecute.ProtocolReadParameterId, Convert.ToString(setToExecute.ValueToSet), setToExecute.NumberOfRetries, primaryKey))
            {
                Log(nameof(ValidateProfileParameter), $"SET verified on '{resourceElement.ElementName}': the value '{setToExecute.ValueToSet}' is set on parameter '{setToExecute.Description}'");
                return true;
            }

            Log(nameof(ValidateProfileParameter), $"Failed to verified SET on '{resourceElement.ElementName}': the value '{setToExecute.ValueToSet}' is NOT set on parameter '{setToExecute.Description}'");
            return false;
        }

        /// <summary>
        ///     Validates the set of the profile parameter with a max amount of retries.
        /// </summary>
        /// <param name="resourceElement">The element of the resource.</param>
        /// <param name="paramId">The Parameter ID.</param>
        /// <param name="value">The set value.</param>
        /// <param name="retries">The number of retries.</param>
        /// <param name="primaryKey">
        ///     Optional; Indicates if the validation needs to be done on a table row, <see langword="null" />
        ///     if none.
        /// </param>
        /// <returns>The indication of the validation was  successful.</returns>
        private bool ValidateProfileParameterWithMaxRetries(Element resourceElement, int paramId, string value, int retries, string primaryKey = null)
        {
            var succeeded = false;
            for (var i = 0; i < retries; i++)
            {
                string raw = resourceElement.GetRawValue(paramId, value);
                string rawValue = string.IsNullOrWhiteSpace(raw) ? value : raw;
                var checkValue = Convert.ToString(string.IsNullOrWhiteSpace(primaryKey) ? resourceElement.GetParameter(paramId) : resourceElement.GetParameterByPrimaryKey(paramId, primaryKey));

                if (checkValue == rawValue)
                {
                    succeeded = true;
                    break;
                }

                Thread.Sleep(1000);
            }

            return succeeded;
        }

        private void Initialize()
        {
            srmResourceConfigurationInfo = LoadResourceConfigurationInfo();
            if (srmResourceConfigurationInfo == null)
            {
                Log(nameof(Initialize), $"Resource configuration info could not be generated, is needed to gather actual function data.");
				return;
			}

			if (srmResourceConfigurationInfo.OrchestrationLogger is OrchestrationLogger orchestrationLogger)
			{
				helpers.AddOrderReferencesForLogging(orchestrationLogger.ReservationId);
			}
			else if(srmResourceConfigurationInfo.ReservationGuid != Guid.Empty)
			{
				helpers.AddOrderReferencesForLogging(srmResourceConfigurationInfo.ReservationGuid);
			}
			else if(Guid.TryParse(srmResourceConfigurationInfo.BookingManagerInfo?.TableIndex, out var parsedGuid))
			{
				helpers.AddOrderReferencesForLogging(parsedGuid);
			}
			else
			{
				Log(nameof(Initialize), $"Could not find ID of reservation.");
			}

            profileParameterHelper = new ProfileParameterEntryHelper((Engine)helpers.Engine, srmResourceConfigurationInfo.OrchestrationLogger);

            var nodeProfileConfiguration = LoadNodeProfileConfiguration();

            var parametersConfiguration = profileParameterHelper.GetNodeSrmParametersConfiguration(srmResourceConfigurationInfo, nodeProfileConfiguration, true);

            setsToExecuteOnDevice = GetSetsToExecuteOnDevice(helpers, srmResourceConfigurationInfo, parametersConfiguration);

            GetResourceElementProtocolInfo(setsToExecuteOnDevice.ResourceElement);
        }

        private static SetsToExecuteOnDevice GetSetsToExecuteOnDevice(Helpers helpers, SrmResourceConfigurationInfo srmResourceConfiguration, IEnumerable<SrmParameterConfiguration> srmParameters)
		{
            var resource = DataMinerInterface.ResourceManager.GetResource(helpers, srmResourceConfiguration.ResourceId) as FunctionResource ?? throw new NotFoundException($"Could not find resource with ID {srmResourceConfiguration.ResourceId}");

			var resourceElement = DataMinerInterface.Engine.FindElement(helpers, helpers.Engine, resource.DmaID, resource.ElementID) ?? throw new NotFoundException($"Could not find element with ID {resource.DmaID}/{resource.ElementID}");

            List<ISetToExecute> setsToExecute;

			switch (resourceElement.ProtocolName)
			{
                case "Ericsson RX8200.Demodulating (Ericsson RX8200)":
                    setsToExecute = EricssonRX8200DemodulatingSets.GetOrderedSetsToExecute(resource, srmParameters);
                    break;

                case "Ericsson RX8200.Decoding (Ericsson RX8200)":
                    setsToExecute = EricssonRx8200DecodingSets.GetOrderedSetsToExecute(helpers, resource, srmParameters);
                    break;

				case "Novelsat NS2000.Demodulating (Novelsat NS2000)":
					setsToExecute = NovelsatNs2000DemodulatingSets.GetOrderedSetsToExecute(resource, srmParameters);
					break;

				default:
                    throw new NotSupportedException($"Protocol Name {resourceElement.ProtocolName} is not supported.");
			}

            return new SetsToExecuteOnDevice
            {
                ResourceElement = resourceElement,
                SetsToExecute = setsToExecute,
            };
		}

        /// <summary>
		/// Loads the profile instance.
		/// </summary>
		/// <returns>The <see cref="ProfileInstance"/> object.</returns>
		/// <exception cref="ArgumentException">In case there is no 'ProfileInstance' input parameter defined.</exception>
		private NodeProfileConfiguration LoadNodeProfileConfiguration()
        {
            var instancePlaceHolder = helpers.Engine.GetScriptParam("ProfileInstance") ?? throw new ArgumentException("There is no input parameter named ProfileInstance");
            
            try
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, Guid>>(instancePlaceHolder.Value);

                var nodeProfileConfiguration = new NodeProfileConfiguration(data);

                return nodeProfileConfiguration;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("Invalid input parameter 'ProfileInstance': \r\n{0}", ex));
            }
        }

        /// <summary>
        /// Loads resource configuration info object.
        /// </summary>
        /// <returns>The <see cref="SrmResourceConfigurationInfo"/> object.</returns>
        /// <exception cref="ArgumentException">In case there is no 'Info' input parameter defined.</exception>
        private SrmResourceConfigurationInfo LoadResourceConfigurationInfo()
        {
            var infoPlaceHolder = helpers.Engine.GetScriptParam("Info") ?? throw new ArgumentException("There is no input parameter named Info");        

            return JsonConvert.DeserializeObject<SrmResourceConfigurationInfo>(infoPlaceHolder.Value, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
            });
        }

        /// <summary>
        /// Apply configurations at pre roll start time of the booking.
        /// </summary>
        private void ApplyProfileConfigurations()
        {
			using (StartPerformanceLogging())
			{
				foreach (var setToExecute in setsToExecuteOnDevice.SetsToExecute)
				{
					if (!setToExecute.ShouldSetValue)
					{
						Log(nameof(ApplyProfileConfigurations), $"Parameter {setToExecute.Description} should not be set according to the configuration.");
						continue;
					}

					SetAndVerifyParameter(setToExecute, setsToExecuteOnDevice.ResourceElement);
				}
			}
        }

		private void SetAndVerifyParameter(ISetToExecute setToExecute, Element resourceElement)
		{
            Log(nameof(SetAndVerifyParameter), $"Trying to set {setToExecute.Description} to value '{setToExecute.ValueToSet.ToString()}' on resource element {resourceElement.ElementName}");

            if (!SetParameter(setToExecute, resourceElement))
            {
                Log(nameof(SetAndVerifyParameter), $"Unable or not required to set {setToExecute.Description} on device");
                return;
            }

            if (!VerifySetOnParameter(setToExecute, resourceElement))
            {
                throw new ResourceProfileParameterSetFailedException($"Failed to set SRM profile parameter {setToExecute.Description} to value {setToExecute.ValueToSet} on resource element {resourceElement.ElementName}");
            }
        }
    }
}
