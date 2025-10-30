--!strict

local Players = game:GetService("Players")

export type Config = {
smoothingAlpha: number?,
prioritizationWeights: { [string]: number }?,
}

export type ParticipantSnapshot = {
Name: string,
Health: number,
Distance: number,
Attributes: { [string]: number },
}

local DEFAULT_WEIGHTS = {
health = 0.4,
distance = 0.35,
level = 0.25,
}

local AnalyticsHub = {}
AnalyticsHub.__index = AnalyticsHub

function AnalyticsHub.new(config: Config?)
local self = setmetatable({}, AnalyticsHub)
self._config = config or {}
self._weights = self._config.prioritizationWeights or DEFAULT_WEIGHTS
self._smoothingAlpha = self._config.smoothingAlpha or 0.2
self._snapshots = {}
self._smoothed = {}
self._changed = Instance.new("BindableEvent")
return self
end

function AnalyticsHub:Init()
Players.PlayerAdded:Connect(function(player)
self:_trackPlayer(player)
end)
for _, player in Players:GetPlayers() do
self:_trackPlayer(player)
end
end

function AnalyticsHub:Start()
-- Additional asynchronous setup could go here
end

function AnalyticsHub:_trackPlayer(player: Player)
self._snapshots[player] = {
Name = player.DisplayName,
Health = 100,
Distance = math.huge,
Attributes = {},
}
player:GetPropertyChangedSignal("Character"):Connect(function()
self._snapshots[player] = self:_buildSnapshot(player)
end)
end

function AnalyticsHub:_buildSnapshot(player: Player): ParticipantSnapshot
local character = player.Character
local root = character and character:FindFirstChild("HumanoidRootPart")
local humanoid = character and character:FindFirstChildOfClass("Humanoid")
local camera = workspace.CurrentCamera
local distance = math.huge
if root and camera then
distance = (camera.CFrame.Position - root.Position).Magnitude
end

return {
Name = player.DisplayName,
Health = humanoid and humanoid.Health or 0,
Distance = distance,
Attributes = {
Stamina = player:GetAttribute("Stamina") or 0,
Level = player:GetAttribute("Level") or 1,
},
}
end

local function smooth(current: ParticipantSnapshot, previous: ParticipantSnapshot?, alpha: number): ParticipantSnapshot
if not previous then
return current
end
local blendedAttributes = {}
for key, value in current.Attributes do
local last = previous.Attributes[key] or value
blendedAttributes[key] = last + (value - last) * alpha
end

return {
Name = current.Name,
Health = previous.Health + (current.Health - previous.Health) * alpha,
Distance = previous.Distance + (current.Distance - previous.Distance) * alpha,
Attributes = blendedAttributes,
}
end

function AnalyticsHub:_prioritize(snapshot: ParticipantSnapshot): number
local healthScore = (snapshot.Health / 100) * (self._weights.health or DEFAULT_WEIGHTS.health)
local distanceScore = (1 / (snapshot.Distance + 1)) * (self._weights.distance or DEFAULT_WEIGHTS.distance)
local level = snapshot.Attributes.Level or 1
local levelScore = (level / 100) * (self._weights.level or DEFAULT_WEIGHTS.level)
return healthScore + distanceScore + levelScore
end

function AnalyticsHub:Update(dt: number)
for player, _ in self._snapshots do
local snapshot = self:_buildSnapshot(player)
self._snapshots[player] = snapshot
self._smoothed[player] = smooth(snapshot, self._smoothed[player], self._smoothingAlpha)
end

local ranked = {}
for _, snapshot in self._smoothed do
table.insert(ranked, snapshot)
end
table.sort(ranked, function(a, b)
return self:_prioritize(a) > self:_prioritize(b)
end)

self._changed:Fire(ranked)
end

function AnalyticsHub:GetRankedSnapshots(): { ParticipantSnapshot }
local list = {}
for _, snapshot in self._smoothed do
table.insert(list, snapshot)
end
return list
end

function AnalyticsHub:Changed()
return self._changed.Event
end

function AnalyticsHub:Destroy()
self._changed:Destroy()
end

return AnalyticsHub
