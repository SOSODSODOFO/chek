--!strict

local PreviewModule = {}
PreviewModule.__index = PreviewModule

function PreviewModule.new()
local self = setmetatable({}, PreviewModule)
self._model = Instance.new("Model")
self._model.Name = "ChekPreviewModel"
self._mount = Instance.new("Part")
self._mount.Name = "PreviewMount"
self._mount.Anchored = true
self._mount.CanCollide = false
self._mount.Transparency = 1
self._mount.Size = Vector3.new(1, 1, 1)
self._mount.Parent = self._model
self._previewCamera = Instance.new("Camera")
self._previewCamera.FieldOfView = 60
self._model.Parent = workspace
self._active = false
self._movement = nil
self._visualization = nil
return self
end

function PreviewModule:Init(movementModule, visualizationModule)
self._movement = movementModule
self._visualization = visualizationModule
self._active = true
end

function PreviewModule:Start()
-- Could load default preview assets
end

function PreviewModule:Update(dt: number)
if not self._active then
return
end
local state = self._movement and self._movement:GetState()
if state then
self._mount.CFrame = CFrame.new(Vector3.new(0, state.Altitude * 0.1, 0)) * CFrame.Angles(0, math.rad(tick() * 10 % 360), 0)
end
end

function PreviewModule:Destroy()
self._model:Destroy()
self._previewCamera:Destroy()
end

return PreviewModule
