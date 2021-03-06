﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace DataAggregation.NationalService
{
    using DataAggregation.Common.Types;
    using DataAggregation.NationalService.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Votes controller.
    /// </summary>
    public class NationalStatsController : Controller
    {

        private const string HealthStatusDictionary = "healthStatusDictionary";
        private const string TimeStatsDictionary = "TimeTracker";
        private readonly ConcurrentDictionary<string, long> statsDictionary;

        private readonly IReliableStateManager stateManager;

        public NationalStatsController(IReliableStateManager stateManager, ConcurrentDictionary<string, long> statsDictionary)
        {
            this.stateManager = stateManager;
            this.statsDictionary = statsDictionary;
        }

        /// <summary>
        /// GET /votes/counties
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("national/stats")]
        public async Task<NationalStatsViewModel> Get()
        {
            try
            {
                IReliableDictionary<string, DateTimeOffset> timeDictionary =
                    await this.stateManager.GetOrAddAsync<IReliableDictionary<string, DateTimeOffset>>(TimeStatsDictionary);

                DateTimeOffset offset = DateTimeOffset.MinValue;
                IList<KeyValuePair<int, NationalCountyStats>> items = new List<KeyValuePair<int, NationalCountyStats>>();

                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    ConditionalValue<DateTimeOffset> creationTimeResult = await timeDictionary.TryGetValueAsync(tx, "StartTime");

                    if (creationTimeResult.HasValue)
                    {
                        offset = creationTimeResult.Value;
                    }

                    var model = new NationalStatsViewModel(
                        this.statsDictionary["totalDoctors"],
                        this.statsDictionary["totalPatientCount"],
                        this.statsDictionary["totalHealthReportCount"],
                        offset);

                    return model;
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.Message(e.ToString());
                throw;
            }
        }
    }
}