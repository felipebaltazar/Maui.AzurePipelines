﻿using PipelineApproval.Abstractions;

namespace PipelineApproval.Infrastructure.Services;

public sealed class PreferencesService : IPreferencesService
{
    public string Get(string key, string defaultValue) =>
        Preferences.Get(key, defaultValue);

    public IPreferencesService Set(string key, string value)
    {
        Preferences.Set(key, value);
        return this;
    }
}
