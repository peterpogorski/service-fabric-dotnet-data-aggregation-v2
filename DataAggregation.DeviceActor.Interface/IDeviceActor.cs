﻿namespace DataAggregation.DeviceCreationService
{

    using DataAggregation.Common.Types;
    using Microsoft.ServiceFabric.Actors;
    using System;
    using System.Threading.Tasks;

    public interface IDeviceActor : IActor
    {
        Task NewAsync(DeviceInfo info);

        Task<DeviceDataViewModel> GetDeviceDataAsync();

        Task<Guid> GetAssociatedDoctorAsync();

    }
}