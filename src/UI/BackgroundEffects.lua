--!strict

local RunService = game:GetService("RunService")

export type Config = {
modes: { string }?,
defaultMode: string?,
intensity: number?,
}

local DEFAULT_CONFIG: Config = {
modes = { "Particles", "Wave", "Geometry" },
defaultMode = "Particles",
intensity = 0.6,
}

local BackgroundEffects = {}
BackgroundEffects.__index = BackgroundEffects

function BackgroundEffects.new(config: Config?)
local merged = table.clone(DEFAULT_CONFIG)
if config then
for key, value in config do
merged[key] = value
end
end

local self = setmetatable({}, BackgroundEffects)
self._config = merged
self._mode = merged.defaultMode
self._container = Instance.new("Folder")
self._container.Name = "ChekBackgrounds"
local camera = workspace.CurrentCamera
if camera then
self._container.Parent = camera
end
self._intensity = merged.intensity
self._tick = 0
return self
end

function BackgroundEffects:Init()
self:_buildInitialEffects()
end

function BackgroundEffects:_buildInitialEffects()
if self._mode == "Particles" then
self:_spawnParticleEmitter()
elseif self._mode == "Wave" then
self:_spawnWaveMesh()
elseif self._mode == "Geometry" then
self:_spawnGeometryField()
end
end

function BackgroundEffects:_spawnParticleEmitter()
local part = Instance.new("Part")
part.Name = "ParticleEmitterAnchor"
part.Anchored = true
part.CanCollide = false
part.Transparency = 1
part.Size = Vector3.new(1, 1, 1)
part.Parent = self._container

local emitter = Instance.new("ParticleEmitter")
emitter.Name = "BackgroundParticles"
emitter.Speed = NumberRange.new(0, 2 + self._intensity * 4)
emitter.Rate = 24 * self._intensity
emitter.Lifetime = NumberRange.new(4, 8)
emitter.Texture = "rbxassetid://particles_texture"
emitter.Parent = part
end

function BackgroundEffects:_spawnWaveMesh()
local part = Instance.new("Part")
part.Name = "WaveAnchor"
part.Anchored = true
part.Size = Vector3.new(50, 1, 50)
part.Transparency = 0.85
part.Color = Color3.fromRGB(24, 120, 255)
part.Material = Enum.Material.Neon
part.CanCollide = false
part.Parent = self._container
end

function BackgroundEffects:_spawnGeometryField()
for index = 1, 24 do
local part = Instance.new("Part")
part.Name = string.format("GeoShape%02d", index)
part.Size = Vector3.new(1, 1, 1) * math.random(1, 4)
part.Anchored = true
part.CanCollide = false
part.Shape = Enum.PartType.Ball
part.Color = Color3.fromHSV(index / 24, 0.6, 1)
part.Transparency = 0.45
part.Parent = self._container
end
end

function BackgroundEffects:SetMode(mode: string)
if not table.find(self._config.modes, mode) then
return
end
self._mode = mode
self._container:ClearAllChildren()
self:_buildInitialEffects()
end

function BackgroundEffects:SetIntensity(intensity: number)
self._intensity = math.clamp(intensity, 0, 1)
self._container:ClearAllChildren()
self:_buildInitialEffects()
end

function BackgroundEffects:Start()
-- Nothing extra required yet
end

function BackgroundEffects:Update(dt: number)
self._tick += dt
if self._mode == "Wave" then
for _, part in self._container:GetChildren() do
if part:IsA("BasePart") then
part.Size = Vector3.new(50, 1 + math.sin(self._tick) * self._intensity, 50)
end
end
elseif self._mode == "Geometry" then
for index, part in self._container:GetChildren() do
if part:IsA("BasePart") then
local angle = (index / #self._container:GetChildren()) * math.pi * 2
part.CFrame = CFrame.new(math.cos(self._tick + angle) * 10, math.sin(self._tick + angle) * 4, math.sin(self._tick + angle) * 10)
end
end
end
end

function BackgroundEffects:Destroy()
self._container:Destroy()
end

return BackgroundEffects
