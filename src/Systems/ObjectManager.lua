--!strict

local CollectionService = game:GetService("CollectionService")

export type ObjectDescriptor = {
tag: string?,
name: string?,
autoAnchor: boolean?,
highlight: boolean?,
}

local ObjectManager = {}
ObjectManager.__index = ObjectManager

function ObjectManager.new()
local self = setmetatable({}, ObjectManager)
self._descriptors = {}
self._tracked = {}
self._signal = Instance.new("BindableEvent")
self._connections = {}
return self
end

function ObjectManager:Register(descriptor: ObjectDescriptor)
if not descriptor.tag then
return
end
self._descriptors[descriptor.tag] = descriptor
self:_scan(descriptor.tag)
if self._connections[descriptor.tag] then
self._connections[descriptor.tag]:Disconnect()
end
self._connections[descriptor.tag] = CollectionService:GetInstanceAddedSignal(descriptor.tag):Connect(function(instance)
self:_applyDescriptor(instance, descriptor)
end)
end

function ObjectManager:_scan(tag: string)
local descriptor = self._descriptors[tag]
if not descriptor then
return
end
for _, instance in CollectionService:GetTagged(tag) do
self:_applyDescriptor(instance, descriptor)
end
end

function ObjectManager:_applyDescriptor(instance: Instance, descriptor: ObjectDescriptor)
if descriptor.name then
instance.Name = descriptor.name
end
if descriptor.autoAnchor and instance:IsA("BasePart") then
instance.Anchored = true
end
if descriptor.highlight then
local highlight = Instance.new("Highlight")
highlight.Name = "ChekObjectHighlight"
highlight.Adornee = instance
highlight.FillTransparency = 1
highlight.OutlineColor = Color3.fromRGB(255, 255, 255)
highlight.Parent = instance
end
self._tracked[instance] = descriptor
self._signal:Fire(instance)
end

function ObjectManager:OnObjectManaged()
return self._signal.Event
end

function ObjectManager:Destroy()
self._signal:Destroy()
for _, connection in self._connections do
connection:Disconnect()
end
end

return ObjectManager
