namespace Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Eurovision
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Exceptions;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Integrations.Eurovision;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Service;
    using Skyline.DataMiner.DeveloperCommunityLibrary.YLE.Utilities;
    using Service = Service.Service;
    using Type = Integrations.Eurovision.Type;

    public class EurovisionBookingHandler
    {
        private readonly Helpers helpers;
        private readonly IActionableElement ebuElement;

        public EurovisionBookingHandler(Helpers helpers, string userGroup)
        {
            this.helpers = helpers ?? throw new ArgumentNullException(nameof(helpers));

            string ebuElementName = helpers.OrderManagerElement.GetEurovisionElementName(userGroup);
            ebuElement = String.IsNullOrWhiteSpace(ebuElementName) ? null : helpers.Engine.FindElement(ebuElementName);
            if (ebuElement == null) throw new ElementNotFoundException($"No EBU element found for user group {userGroup} with name {ebuElementName}");
        }

        public event EventHandler<string> ProgressReported;

        public EurovisionBookingResult BookEurovisionService(Service service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            if (service.EurovisionBookingDetails == null) throw new NotSupportedException($"Unable to create booking if EurovisionBookingDetails are not available");

            EurovisionBookingResult bookingResult;
            switch (service.EurovisionBookingDetails.Type)
            {
                case Type.NewsEvent:
                    bookingResult = BookNewsEvent(service.EurovisionBookingDetails, service.Start, service.End);
                    break;
                case Type.ProgramEvent:
                    bookingResult = BookProgramEvent(service.EurovisionBookingDetails, service.Start, service.End);
                    break;
                case Type.SatelliteCapacity:
                    bookingResult = BookSatelliteCapacity(service.EurovisionBookingDetails, service.Start, service.End);
                    break;
                case Type.OSSTransmission:
                    bookingResult = BookOssTransmission(service.EurovisionBookingDetails, service.Start, service.End);
                    break;
                case Type.UnilateralTransmission:
                    bookingResult = BookUnilateralTransmission(service.EurovisionBookingDetails, service.Start, service.End);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported type: {service.EurovisionBookingDetails.Type}");
            }

            return bookingResult;
        }

        private EurovisionBookingResult BookNewsEvent(EurovisionBookingDetails details, DateTime start, DateTime end)
        {
			ReportProgress("Booking EBU News Event...");

			// Create customer order
			ReportProgress("Creating Customer Order...");

			var customerOrderReference = Convert.ToString(Guid.NewGuid());
			if (!NewsEvent_CreateCustomerOrder(details, customerOrderReference, out string customerOrderKey))
            {
				ReportProgress("Creating customer order failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating customer order failed"
				};
			}

			ReportProgress("Creating customer order was successful");

			// Create customer contact
			if (!String.IsNullOrEmpty(details.Contact))
			{
				ReportProgress("Creating Customer Order Contact...");

				if (!NewsEvent_CreateCustomerOrderContact(details, customerOrderReference, customerOrderKey, out string key))
                {
					ReportProgress("Creating customer order contact failed");

					return new EurovisionBookingResult
					{
						IsSuccessful = false,
						ErrorMessage = "Creating customer order contact failed"
					};
				}

				ReportProgress("Creating customer order contact was successful");
			}

			// Create Transmission
			ReportProgress("Creating Transmission...");

			if (!NewsEvent_CreateTransmission(details, customerOrderKey, start, end, out string transmissionKey))
			{
				ReportProgress("Creating transmission failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating transmission failed"
				};
			}

			ReportProgress("Creating transmission was successful");

			// Create Destination
			ReportProgress("Adding Destination...");

			if (!NewsEvent_CreateDestination(details, transmissionKey, out string destinationKey))
			{
				ReportProgress("Creating destination failed");

				// delete the transmission
				ebuElement.SetParameterByPrimaryKey(7233, transmissionKey, "1");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating destination failed"
				};
			}

			ReportProgress("Creating destination was successful");

			// Add facility
			if (!String.IsNullOrWhiteSpace(details.FacilityProductId))
			{
				ReportProgress("Adding Facility...");

				if (!NewsEvent_AddFacility(details, transmissionKey, destinationKey, start, end, out string facilityKey))
				{
					return new EurovisionBookingResult
					{
						IsSuccessful = false,
						ErrorMessage = "Adding facility failed"
					};
				}

				ReportProgress("Adding facility was successful");
			}

			return new EurovisionBookingResult
			{
				IsSuccessful = true,
				Id = transmissionKey
			};
		}

		private bool NewsEvent_CreateCustomerOrder(EurovisionBookingDetails details, string customerOrderReference, out string customerOrderKey)
        {
			var createCustomerOrderRequest = JsonConvert.SerializeObject(new CreateCustomerOrderRequest
			{
				Id = customerOrderReference,
				Request = new CreateCustomerOrderRequest.CustomerOrder
				{
					BillTo = details.DestinationOrganizationCode,
					EventNumber = details.EventNumber,
					Reference = customerOrderReference,
					Note = "",
					Draft = false
				}
			});
			ebuElement.SetParameter(61, createCustomerOrderRequest);

			// check if order is created
			return EurovisionExtensions.CheckForSuccessfulJsonResponse(ebuElement, customerOrderReference, out customerOrderKey);
		}

		private bool NewsEvent_CreateCustomerOrderContact(EurovisionBookingDetails details, string customerOrderReference, string customerOrderKey, out string key)
        {
			var customerOrderContactReference = Convert.ToString(Guid.NewGuid());
			var createCustomerOrderContactRequest = JsonConvert.SerializeObject(new CreateCustomerOrderContactRequest
			{
				Id = customerOrderContactReference,
				CustomerOrder = customerOrderKey,
				Request = new CreateCustomerOrderContactRequest.Contact
				{
					Email = details.Contact
				}
			});
			ebuElement.SetParameter(75, createCustomerOrderContactRequest);

			return EurovisionExtensions.CheckForSuccessfulJsonResponse(ebuElement, customerOrderReference, out key);
		}

		private bool NewsEvent_CreateTransmission(EurovisionBookingDetails details, string customerOrderKey, DateTime start, DateTime end, out string transmissionKey)
        {
			var transmissionReference = Guid.NewGuid().ToString();
			DateTime startTimeUtc = start.ToUniversalTime();
			DateTime endTimeUtc = end.ToUniversalTime();
			var createTransmissionRequest = JsonConvert.SerializeObject(new CreateTransmissionRequest
			{
				Id = transmissionReference,
				CustomerOrder = customerOrderKey,
				Request = new CreateTransmissionRequest.Transmission
				{
					EventNumber = details.EventNumber,
					ProductCode = "UNI",
					StartDate = startTimeUtc,
					StartTime = startTimeUtc.ToString("HH:mm", CultureInfo.InvariantCulture),
					EndTime = endTimeUtc.ToString("HH:mm", CultureInfo.InvariantCulture),
					Nature1 = details.Description ?? "",
					Nature2 = "",
					OriginVenue = "venue",
					FeedPointId = details.FeedpointId,
					VideoDetails = new CreateTransmissionRequest.Transmission.Video
					{
						VideoDefinitionCode = details.VideoDefinitionCode,
						VideoResolutionCode = details.VideoDefinitionCode != "SD" ? details.VideoResolutionCode : null,
						VideoAspectRatioCode = details.VideoDefinitionCode == "SD" ? details.VideoAspectRatioCode : null,
						VideoBitrateCode = details.VideoBitrateCode,
						VideoFrameRateCode = details.VideoFrameRateCode
					},
					AudioDetails = new[]
					{
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel1.AudioChannelCode,
							Text = details.AudioChannel1.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel2.AudioChannelCode,
							Text = details.AudioChannel2.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel3.AudioChannelCode,
							Text = details.AudioChannel3.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel4.AudioChannelCode,
							Text = details.AudioChannel4.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel5.AudioChannelCode,
							Text = details.AudioChannel5.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel6.AudioChannelCode,
							Text = details.AudioChannel6.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel7.AudioChannelCode,
							Text = details.AudioChannel7.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel8.AudioChannelCode,
							Text = details.AudioChannel8.AudioChannelOtherText
						}
					},
					Reference = transmissionReference,
					Note = details.Note,
					ContractCode = details.ContractCode
				}
			});
			ebuElement.SetParameter(63, createTransmissionRequest);

			// check if transmission is created
			return EurovisionExtensions.CheckForSuccessfulJsonResponse(ebuElement, transmissionReference, out transmissionKey);
		}

		private bool NewsEvent_CreateDestination(EurovisionBookingDetails details, string transmissionKey, out string destinationKey)
        {
			var destinationReference = Convert.ToString(Guid.NewGuid());
			var createDestinationRequest = JsonConvert.SerializeObject(new CreateDestinationRequest
			{
				Id = destinationReference,
				Transmission = transmissionKey,
				Request = new CreateDestinationRequest.Destination
				{
					OrganizationCode = details.DestinationOrganizationCode,
					CityCode = details.DestinationCityCode
				}

			});
			ebuElement.SetParameter(65, createDestinationRequest);

			// check if destination is created
			return EurovisionExtensions.CheckForSuccessfulXmlResponse(ebuElement, helpers.Engine, destinationReference, out destinationKey);
		}

		private bool NewsEvent_AddFacility(EurovisionBookingDetails details, string transmissionKey, string destinationKey, DateTime start, DateTime end, out string facilityKey)
        {
			DateTime startTimeUtc = start.ToUniversalTime();
			DateTime endTimeUtc = end.ToUniversalTime();
			var facilityReference = Convert.ToString(Guid.NewGuid());
			var createAssociatedItemRequest = JsonConvert.SerializeObject(new CreateAssociatedItemRequest
			{
				Id = facilityReference,
				Transmission = transmissionKey,
				Destination = destinationKey,
				Request = new CreateAssociatedItemRequest.Facility
				{
					Id = details.FacilityProductId,
					ProductCode = details.FacilityProductCode,
					BeginDate = startTimeUtc,
					StartTime = String.Format("{0}:{1}", startTimeUtc.Hour, startTimeUtc.Minute.ToString("D2")),
					EndTime = String.Format("{0}:{1}", endTimeUtc.Hour, endTimeUtc.Minute.ToString("D2"))
				}

			});
			ebuElement.SetParameter(67, createAssociatedItemRequest);

			// check if associated item is created
			return EurovisionExtensions.CheckForSuccessfulXmlResponse(ebuElement, helpers.Engine, facilityReference, out facilityKey);

		}

        private EurovisionBookingResult BookProgramEvent(EurovisionBookingDetails details, DateTime start, DateTime end)
        {
			ReportProgress("Booking EBU Program Event...");

			// Create customer order
			ReportProgress("Creating Customer Order...");

			var customerOrderReference = Convert.ToString(Guid.NewGuid());
			if (!NewsEvent_CreateCustomerOrder(details, customerOrderReference, out string customerOrderKey))
			{
				ReportProgress("Creating customer order failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating customer order failed"
				};
			}

			ReportProgress("Creating customer order was successful");

			// Create customer contact
			if (!String.IsNullOrEmpty(details.Contact))
			{
				ReportProgress("Creating Customer Order Contact...");

				if (!NewsEvent_CreateCustomerOrderContact(details, customerOrderReference, customerOrderKey, out string key))
				{
					ReportProgress("Creating customer order contact failed");

					return new EurovisionBookingResult
					{
						IsSuccessful = false,
						ErrorMessage = "Creating customer order contact failed"
					};
				}

				ReportProgress("Creating customer order contact was successful");
			}

			// Create participation
			ReportProgress("Creating Participation...");

			if (!ProgramEvent_CreateParticipation(details, customerOrderKey, start, end, out string transmissionKey))
			{
				ReportProgress("Creating Participation failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating participation failed"
				};
			}

			ReportProgress("Creating Participation was successful");

			return new EurovisionBookingResult
			{
				IsSuccessful = true,
				Id = transmissionKey
			};
		}

		private bool ProgramEvent_CreateParticipation(EurovisionBookingDetails details, string customerOrderKey, DateTime start, DateTime end, out string transmissionKey)
        {
			var participationReference = Convert.ToString(Guid.NewGuid());
			DateTime startTimeUtc = start.ToUniversalTime(); // Participation timing is in UTC
			DateTime endTimeUtc = end.ToUniversalTime();
			var createTransmissionRequest = JsonConvert.SerializeObject(new CreateParticipationRequest
			{
				Id = participationReference,
				CustomerOrder = customerOrderKey,
				Request = new CreateParticipationRequest.Participation
				{
					StartDate = startTimeUtc,
					StartTime = startTimeUtc.ToString("HH:mm", CultureInfo.InvariantCulture),
					EndTime = endTimeUtc.ToString("HH:mm", CultureInfo.InvariantCulture),
					TransmissionNumber = details.MultilateralTransmissionNumber,
					EventNumber = details.EventNumber,
					System = "",
					Reference = participationReference,
					Organization = details.DestinationOrganizationCode,
					BillTo = details.DestinationOrganizationCode,
					City = details.DestinationCityCode,
					Bureau = "",
					Via = "",
					Note = details.Note,
					Contract = details.ContractCode
				}
			});
			ebuElement.SetParameter(69, createTransmissionRequest);

			// check if transmission is created
			return EurovisionExtensions.CheckForSuccessfulJsonResponse(ebuElement, participationReference, out transmissionKey);
		}

        private EurovisionBookingResult BookSatelliteCapacity(EurovisionBookingDetails details, DateTime start, DateTime end)
        {
			ReportProgress("Booking EBU Satellite Capacity...");

			// Create customer order
			ReportProgress("Creating Customer Order...");

			var customerOrderReference = Convert.ToString(Guid.NewGuid());
			if (!SatelliteCapacity_CreateCustomerOrder(details, customerOrderReference, out string customerOrderKey))
			{
				ReportProgress("Creating customer order failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating customer order failed"
				};
			}

			ReportProgress("Creating customer order was successful");

			// Create customer order contact
			bool createCustomerOrderContact = !String.IsNullOrEmpty(details.ContactFirstName) || !String.IsNullOrEmpty(details.ContactLastName) || !String.IsNullOrEmpty(details.Email) || !String.IsNullOrEmpty(details.Phone);
			if (createCustomerOrderContact)
			{
				ReportProgress("Creating Customer Order Contact...");

				if (!SatelliteCapacity_CreateCustomerOrderContact(details, customerOrderReference, customerOrderKey, out string key))
				{
					ReportProgress("Creating Customer Order Contact failed");

					return new EurovisionBookingResult
					{
						IsSuccessful = false,
						ErrorMessage = "Creating customer order contact failed"
					};
				}

				ReportProgress("Creating Customer Order Contact was successful");
			}

			// Create Transmission
			ReportProgress("Creating Transmsission...");

			if (!SatelliteCapacity_CreateTransmission(details, customerOrderKey, start, end, out string transmissionKey))
			{
				ReportProgress("Creating Transmission failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating transmission failed"
				};
			}

			ReportProgress("Creating Transmission was successful");

			// Create Technical System
			ReportProgress("Creating Technical System...");

			if (!SatelliteCapacity_CreateTechnicalSystem(details, transmissionKey, out string technicalSystemKey))
            {
				ReportProgress("Creating Technical System. failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating technical system failed"
				};
			}

			ReportProgress("Creating Technical System was successful");

			// Create temporary contact
			ReportProgress("Creating Temporary Contact...");

			bool createTemporaryContact = !String.IsNullOrEmpty(details.ContactFirstName) || !String.IsNullOrEmpty(details.ContactLastName) || !String.IsNullOrEmpty(details.Email) || !String.IsNullOrEmpty(details.Phone);
			if (createTemporaryContact && !SatelliteCapacity_CreateTemporaryContact(details, transmissionKey, out string temporaryContactKey))
			{
				// delete the transmission
				ebuElement.SetParameterByPrimaryKey(7233, transmissionKey, "1");

				ReportProgress("Creating Temporary Contact failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating temporary contact failed"
				};
			}

			ReportProgress("Creating Temporary Contact was successful");

			return new EurovisionBookingResult
			{
				IsSuccessful = true,
				Id = transmissionKey
			};
		}

		private bool SatelliteCapacity_CreateCustomerOrder(EurovisionBookingDetails details, string customerOrderReference, out string customerOrderKey)
        {
			var createCustomerOrderRequest = JsonConvert.SerializeObject(new CreateCustomerOrderRequest
			{
				Id = customerOrderReference,
				Request = new CreateCustomerOrderRequest.CustomerOrder
				{
					BillTo = details.OriginOrganizationCode,
					Reference = customerOrderReference,
					Note = "",
					Draft = false
				}
			});
			ebuElement.SetParameter(61, createCustomerOrderRequest);

			// check if order is created
			return EurovisionExtensions.CheckForSuccessfulJsonResponse(ebuElement, customerOrderReference, out customerOrderKey);
		}

		private bool SatelliteCapacity_CreateCustomerOrderContact(EurovisionBookingDetails details, string customerOrderReference, string customerOrderKey, out string key)
		{
			var customerOrderContactReference = Convert.ToString(Guid.NewGuid());
			var createCustomerOrderContactRequest = JsonConvert.SerializeObject(new CreateCustomerOrderContactRequest
			{
				Id = customerOrderContactReference,
				CustomerOrder = customerOrderKey,
				Request = new CreateCustomerOrderContactRequest.Contact
				{
					Email = details.Email,
					FirstName = details.ContactFirstName,
					LastName = details.ContactLastName,
					Phone = details.Phone
				}
			});
			ebuElement.SetParameter(75, createCustomerOrderContactRequest);

			return EurovisionExtensions.CheckForSuccessfulJsonResponse(ebuElement, customerOrderReference, out key);
		}

		private bool SatelliteCapacity_CreateTransmission(EurovisionBookingDetails details, string customerOrderKey, DateTime start, DateTime end, out string transmissionKey)
        {
			var transmissionReference = Guid.NewGuid().ToString();
            DateTime startTimeUtc = start.ToUniversalTime();
            DateTime endTimeUtc = end.ToUniversalTime();
            var createTransmissionRequest = JsonConvert.SerializeObject(new CreateTransmissionRequest
			{
				Id = transmissionReference,
				CustomerOrder = customerOrderKey,
				Request = new CreateTransmissionRequest.Transmission
				{
					ProductCode = "SS",
                    StartDate = startTimeUtc,
                    StartTime = startTimeUtc.ToString("HH:mm", CultureInfo.InvariantCulture),
                    EndTime = endTimeUtc.ToString("HH:mm", CultureInfo.InvariantCulture),
                    Title = String.Format("Transmission {0}", transmissionReference),
					FeedPointId = details.FeedpointId,
					OriginOrganizationCode = details.OriginCityCode,
					VideoDetails = new CreateTransmissionRequest.Transmission.Video
					{
						VideoDefinitionCode = details.VideoDefinitionCode,
						VideoResolutionCode = details.VideoResolutionCode,
						VideoBandwidthCode = details.VideoBandwidthCode,
						VideoFrameRateCode = details.VideoFrameRateCode
					},
					AudioDetails = new[]
					{
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel1.AudioChannelCode,
							Text = details.AudioChannel1.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel2.AudioChannelCode,
							Text = details.AudioChannel2.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel3.AudioChannelCode,
							Text = details.AudioChannel3.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel4.AudioChannelCode,
							Text = details.AudioChannel4.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel5.AudioChannelCode,
							Text = details.AudioChannel5.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel6.AudioChannelCode,
							Text = details.AudioChannel6.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel7.AudioChannelCode,
							Text = details.AudioChannel7.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel8.AudioChannelCode,
							Text = details.AudioChannel8.AudioChannelOtherText
						}
					},
					Reference = transmissionReference,
					Note = details.Note,
					ContractCode = details.ContractCode
				}
			});
			ebuElement.SetParameter(63, createTransmissionRequest);

			// check if transmission is created
			return EurovisionExtensions.CheckForSuccessfulJsonResponse(ebuElement, transmissionReference, out transmissionKey);
		}

		private bool SatelliteCapacity_CreateTechnicalSystem(EurovisionBookingDetails details, string transmissionKey, out string technicalSystemKey)
        {
			var technicalSystemReference = Convert.ToString(Guid.NewGuid());
			var createTechnicalSystemRequest = JsonConvert.SerializeObject(new CreateTechnicalSystemRequest
			{
				Id = technicalSystemReference,
				Transmission = transmissionKey,
				Request = new CreateTechnicalSystemRequest.TechnicalSystem
				{
					SystemId = details.SatelliteId,
					VideoDetails = new CreateTechnicalSystemRequest.TechnicalSystem.Video
					{
						VideoDefinitionCode = details.VideoDefinitionCode,
						VideoResolutionCode = details.VideoResolutionCode,
						VideoBandwidthCode = details.VideoBandwidthCode,
						VideoFrameRateCode = details.VideoFrameRateCode
					},
					AudioDetails = new[]
					{
						new CreateTechnicalSystemRequest.TechnicalSystem.Audio
						{
							Code = details.AudioChannel1.AudioChannelCode,
							Text = details.AudioChannel1.AudioChannelOtherText
						},
						new CreateTechnicalSystemRequest.TechnicalSystem.Audio
						{
							Code = details.AudioChannel2.AudioChannelCode,
							Text = details.AudioChannel2.AudioChannelOtherText
						},
						new CreateTechnicalSystemRequest.TechnicalSystem.Audio
						{
							Code = details.AudioChannel3.AudioChannelCode,
							Text = details.AudioChannel3.AudioChannelOtherText
						},
						new CreateTechnicalSystemRequest.TechnicalSystem.Audio
						{
							Code = details.AudioChannel4.AudioChannelCode,
							Text = details.AudioChannel4.AudioChannelOtherText
						},
						new CreateTechnicalSystemRequest.TechnicalSystem.Audio
						{
							Code = details.AudioChannel5.AudioChannelCode,
							Text = details.AudioChannel5.AudioChannelOtherText
						},
						new CreateTechnicalSystemRequest.TechnicalSystem.Audio
						{
							Code = details.AudioChannel6.AudioChannelCode,
							Text = details.AudioChannel6.AudioChannelOtherText
						},
						new CreateTechnicalSystemRequest.TechnicalSystem.Audio
						{
							Code = details.AudioChannel7.AudioChannelCode,
							Text = details.AudioChannel7.AudioChannelOtherText
						},
						new CreateTechnicalSystemRequest.TechnicalSystem.Audio
						{
							Code = details.AudioChannel8.AudioChannelCode,
							Text = details.AudioChannel8.AudioChannelOtherText
						}
					}
				}
			});
			ebuElement.SetParameter(71, createTechnicalSystemRequest);

			// check if order is created
			return EurovisionExtensions.CheckForSuccessfulJsonResponse(ebuElement, technicalSystemReference, out technicalSystemKey);
		}

		private bool SatelliteCapacity_CreateTemporaryContact(EurovisionBookingDetails details, string transmissionKey, out string temporaryContactKey)
        {
			var temporaryContactReference = Convert.ToString(Guid.NewGuid());
			var createTemporaryContactRequest = JsonConvert.SerializeObject(new CreateTemporaryTransmissionContactRequest
			{
				Id = temporaryContactReference,
				Transmission = transmissionKey,
				Request = new CreateTemporaryTransmissionContactRequest.Contact
				{
					Phone = details.Phone,
					Email = details.Email,
					FirstName = details.ContactFirstName,
					LastName = details.ContactLastName
				}
			});

			ebuElement.SetParameter(73, createTemporaryContactRequest);

			// check if order is created
			return EurovisionExtensions.CheckForSuccessfulJsonResponse(ebuElement, temporaryContactReference, out temporaryContactKey);
		}

		private EurovisionBookingResult BookOssTransmission(EurovisionBookingDetails details, DateTime start, DateTime end)
        {
			// Create customer order
			ReportProgress("Creating Customer Order...");

			var customerOrderReference = Convert.ToString(Guid.NewGuid());
			if (!NewsEvent_CreateCustomerOrder(details, customerOrderReference, out string customerOrderKey))
			{
				ReportProgress("Creating customer order failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating customer order failed"
				};
			}

			ReportProgress("Creating customer order was successful");

			// Create customer contact
			if (!String.IsNullOrEmpty(details.Contact))
			{
				ReportProgress("Creating Customer Order Contact...");

				if (!NewsEvent_CreateCustomerOrderContact(details, customerOrderReference, customerOrderKey, out string key))
				{
					ReportProgress("Creating customer order contact failed");

					return new EurovisionBookingResult
					{
						IsSuccessful = false,
						ErrorMessage = "Creating customer order contact failed"
					};
				}

				ReportProgress("Creating customer order contact was successful");
			}

			// Create transmission
			ReportProgress("Creating Transmission...");

			if (!UniOssTransmission_CreateTransmission(details, customerOrderKey, start, end, out string transmissionKey))
            {
				ReportProgress("Creating transmission failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating transmission failed"
				};
			}

			ReportProgress("Creating Transmission was successful");

			// Add destination
			ReportProgress("Adding Destination...");

			if (!UniOssTransmission_AddDestination(details, transmissionKey, start, end, out string destinationKey))
            {
				ReportProgress("Adding destination failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Adding destination failed"
				};
			}

			ReportProgress("Adding Destination was successful");

			// Add Facility
			if (!String.IsNullOrWhiteSpace(details.FacilityProductId) && !String.IsNullOrWhiteSpace(details.FacilityProductCode))
            {
				ReportProgress("Adding Facility...");

				if (!OssTransmission_AddFacility(details, transmissionKey, destinationKey, start, end))
				{
					ReportProgress("Adding facility failed");

					return new EurovisionBookingResult
					{
						IsSuccessful = false,
						ErrorMessage = "Adding facility failed"
					};
				}

				ReportProgress("Adding Facility was successful");
			}

			return new EurovisionBookingResult
			{
				IsSuccessful = true,
				Id = transmissionKey
			};
		}

        private bool UniOssTransmission_CreateTransmission(EurovisionBookingDetails details, string customerOrderKey, DateTime start, DateTime end, out string transmissionKey)
        {
			var transmissionReference = Convert.ToString(Guid.NewGuid());
            DateTime startTimeUtc = start.ToUniversalTime();
            DateTime endTimeUtc = end.ToUniversalTime();
            var createTransmissionRequest = JsonConvert.SerializeObject(new CreateTransmissionRequest
			{
				Id = transmissionReference,
				CustomerOrder = customerOrderKey,
				Request = new CreateTransmissionRequest.Transmission
				{
					ProductCode = "UNI",
                    StartDate = startTimeUtc,
                    StartTime = startTimeUtc.ToString("HH:mm", CultureInfo.InvariantCulture),
                    EndTime = endTimeUtc.ToString("HH:mm", CultureInfo.InvariantCulture),
                    Title = String.Format("Transmission {0}", transmissionReference),
					FeedPointId = details.FeedpointId,
					OriginOrganizationCode = details.OriginOrganizationCode,
					VideoDetails = new CreateTransmissionRequest.Transmission.Video
					{
						VideoDefinitionCode = details.VideoDefinitionCode,
						VideoResolutionCode = details.VideoDefinitionCode != "SD" ? details.VideoResolutionCode : null,
						VideoAspectRatioCode = details.VideoDefinitionCode == "SD" ? details.VideoAspectRatioCode : null,
						VideoBitrateCode = !String.IsNullOrWhiteSpace(details.VideoBitrateCode) ? details.VideoBitrateCode : null,
						VideoFrameRateCode = !String.IsNullOrWhiteSpace(details.VideoFrameRateCode) ? details.VideoFrameRateCode : null
					},
					AudioDetails = new[]
					{
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel1.AudioChannelCode,
							Text = details.AudioChannel1.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel2.AudioChannelCode,
							Text = details.AudioChannel2.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel3.AudioChannelCode,
							Text = details.AudioChannel3.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel4.AudioChannelCode,
							Text = details.AudioChannel4.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel5.AudioChannelCode,
							Text = details.AudioChannel5.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel6.AudioChannelCode,
							Text = details.AudioChannel6.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel7.AudioChannelCode,
							Text = details.AudioChannel7.AudioChannelOtherText
						},
						new CreateTransmissionRequest.Transmission.Audio
						{
							Code = details.AudioChannel8.AudioChannelCode,
							Text = details.AudioChannel8.AudioChannelOtherText
						}
					},
					Note = details.Note,
					ContractCode = details.ContractCode,
					Reference = transmissionReference
				}
			});
			ebuElement.SetParameter(63, createTransmissionRequest);

			// check if transmission is created
			return EurovisionExtensions.CheckForSuccessfulJsonResponse(ebuElement, transmissionReference, out transmissionKey);
		}

		private bool UniOssTransmission_AddDestination(EurovisionBookingDetails details, string transmissionKey, DateTime start, DateTime end, out string destinationKey)
        {
			var destinationReference = Convert.ToString(Guid.NewGuid());
            DateTime startTimeUtc = start.ToUniversalTime();
            DateTime endTimeUtc = end.ToUniversalTime();
            var createDestinationRequest = JsonConvert.SerializeObject(new CreateDestinationRequest
			{
				Id = destinationReference,
				Transmission = transmissionKey,
				Request = new CreateDestinationRequest.Destination
				{
					OrganizationCode = details.DestinationOrganizationCode,
					CityCode = details.DestinationCityCode,
                    StartTime = String.Format("{0}:{1}", startTimeUtc.Hour, startTimeUtc.Minute.ToString("D2")),
                    EndTime = String.Format("{0}:{1}", endTimeUtc.Hour, endTimeUtc.Minute.ToString("D2")),
                }

			});
			ebuElement.SetParameter(65, createDestinationRequest);

			// check if destination is created
			return EurovisionExtensions.CheckForSuccessfulXmlResponse(ebuElement, helpers.Engine, destinationReference, out destinationKey);
		}

		private bool OssTransmission_AddFacility(EurovisionBookingDetails details, string transmissionKey, string destinationKey, DateTime start, DateTime end)
		{
			var facilityReference = Guid.NewGuid().ToString();
            DateTime startTimeUtc = start.ToUniversalTime();
            DateTime endTimeUtc = end.ToUniversalTime();
            var createAssociatedItemRequest = JsonConvert.SerializeObject(new CreateAssociatedItemRequest
			{
				Id = facilityReference,
				Transmission = transmissionKey,
				Destination = destinationKey,
				Request = new CreateAssociatedItemRequest.Facility
				{
					Id = details.FacilityProductId,
					ProductCode = details.FacilityProductCode,
                    BeginDate = startTimeUtc,
                    StartTime = String.Format("{0}:{1}", startTimeUtc.Hour, startTimeUtc.Minute.ToString("D2")),
                    EndTime = String.Format("{0}:{1}", endTimeUtc.Hour, endTimeUtc.Minute.ToString("D2"))
                }

			});
			ebuElement.SetParameter(67, createAssociatedItemRequest);

			// check if associated item is created
			return EurovisionExtensions.CheckForSuccessfulXmlResponse(ebuElement, helpers.Engine, facilityReference, out string facilityKey);
		}

		private EurovisionBookingResult BookUnilateralTransmission(EurovisionBookingDetails details, DateTime start, DateTime end)
        {
			// Create customer order
			ReportProgress("Creating Customer Order...");

			var customerOrderReference = Convert.ToString(Guid.NewGuid());
			if (!NewsEvent_CreateCustomerOrder(details, customerOrderReference, out string customerOrderKey))
			{
				ReportProgress("Creating customer order failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating customer order failed"
				};
			}

			ReportProgress("Creating customer order was successful");

			// Create customer contact
			if (!String.IsNullOrEmpty(details.Contact))
			{
				ReportProgress("Creating Customer Order Contact...");

				if (!NewsEvent_CreateCustomerOrderContact(details, customerOrderReference, customerOrderKey, out string key))
				{
					ReportProgress("Creating customer order contact failed");

					return new EurovisionBookingResult
					{
						IsSuccessful = false,
						ErrorMessage = "Creating customer order contact failed"
					};
				}

				ReportProgress("Creating customer order contact was successful");
			}

			// Create transmission
			ReportProgress("Creating Transmission...");

			if (!UniOssTransmission_CreateTransmission(details, customerOrderKey, start, end, out string transmissionKey))
			{
				ReportProgress("Creating transmission failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Creating transmission failed"
				};
			}

			ReportProgress("Creating Transmission was successful");

			// Add destination
			ReportProgress("Adding Destination...");

			if (!UniOssTransmission_AddDestination(details, transmissionKey, start, end, out string destinationKey))
			{
				ReportProgress("Adding destination failed");

				return new EurovisionBookingResult
				{
					IsSuccessful = false,
					ErrorMessage = "Adding destination failed"
				};
			}

			ReportProgress("Adding Destination was successful");

			return new EurovisionBookingResult
			{
				IsSuccessful = true,
				Id = transmissionKey
			};
		}

        private void ReportProgress(string progress)
        {
            ProgressReported?.Invoke(this, progress);
        }
    }
}
