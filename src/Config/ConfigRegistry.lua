--!strict

local HttpService = game:GetService("HttpService")

export type ProfileConfig = {
name: string,
description: string?,
movement: table?,
environment: table?,
analytics: table?,
visualization: table?,
theme: table?,
background: table?,
}

local ConfigRegistry = {}
ConfigRegistry.__index = ConfigRegistry

function ConfigRegistry.new(initialProfiles: { [string]: ProfileConfig }?)
local self = setmetatable({}, ConfigRegistry)
self._profiles = initialProfiles and table.clone(initialProfiles) or {}
self._activeProfile = nil
self._changedSignal = Instance.new("BindableEvent")
return self
end

function ConfigRegistry:Init()
-- Reserved for loading persisted data later on.
end

function ConfigRegistry:RegisterProfile(profile: ProfileConfig)
assert(profile.name, "Profile must include a name")
local key = profile.name
self._profiles[key] = profile
self._changedSignal:Fire({ type = "registered", profile = key })
end

function ConfigRegistry:RemoveProfile(name: string)
self._profiles[name] = nil
self._changedSignal:Fire({ type = "removed", profile = name })
end

function ConfigRegistry:GetProfile(name: string): ProfileConfig?
return self._profiles[name]
end

function ConfigRegistry:GetProfiles(): { [string]: ProfileConfig }
return table.clone(self._profiles)
end

function ConfigRegistry:ActivateProfile(name: string)
if not self._profiles[name] then
warn("Attempted to activate unknown profile", name)
return
end
self._activeProfile = name
self._changedSignal:Fire({ type = "activated", profile = name })
end

function ConfigRegistry:GetActiveProfile(): ProfileConfig?
if not self._activeProfile then
return nil
end
return self._profiles[self._activeProfile]
end

function ConfigRegistry:SerializeActiveProfile(): string?
local profile = self:GetActiveProfile()
if not profile then
return nil
end
return HttpService:JSONEncode(profile)
end

function ConfigRegistry:Changed()
return self._changedSignal.Event
end

function ConfigRegistry:Destroy()
self._changedSignal:Destroy()
end

return ConfigRegistry
