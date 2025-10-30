--!strict

local TweenService = game:GetService("TweenService")

export type Dependencies = {
movement: any?,
environment: any?,
analytics: any?,
visualization: any?,
themeManager: any?,
automation: any?,
}

export type InitContext = {
themeManager: any?,
}

local ControlPanel = {}
ControlPanel.__index = ControlPanel

function ControlPanel.new(deps: Dependencies)
local self = setmetatable({}, ControlPanel)
self._deps = deps
self._screenGui = Instance.new("ScreenGui")
self._screenGui.Name = "ChekControlPanel"
self._tabs = {}
self._searchBox = nil
self._filterSignal = Instance.new("BindableEvent")
self._animations = {}
return self
end

function ControlPanel:Init(context: InitContext?)
self._screenGui.ResetOnSpawn = false
self._screenGui.IgnoreGuiInset = true
self._screenGui.DisplayOrder = 10
local localPlayer = game.Players.LocalPlayer
if localPlayer then
self._screenGui.Parent = localPlayer:WaitForChild("PlayerGui")
end
self:_buildTabs()
self:_buildSearch()
end

function ControlPanel:_buildTabs()
local tabs = {
{ name = "Movement", icon = "rbxassetid://movement_icon", module = self._deps.movement },
{ name = "Environment", icon = "rbxassetid://environment_icon", module = self._deps.environment },
{ name = "Analytics", icon = "rbxassetid://analytics_icon", module = self._deps.analytics },
{ name = "Visualization", icon = "rbxassetid://visualization_icon", module = self._deps.visualization },
{ name = "Automation", icon = "rbxassetid://automation_icon", module = self._deps.automation },
}

local frame = Instance.new("Frame")
frame.Name = "TabContainer"
frame.Size = UDim2.new(0, 360, 0, 48)
frame.Position = UDim2.new(0, 24, 0, 24)
frame.BackgroundTransparency = 0.2
frame.BackgroundColor3 = Color3.fromRGB(20, 20, 24)
frame.Parent = self._screenGui

local list = Instance.new("UIListLayout")
list.FillDirection = Enum.FillDirection.Horizontal
list.Padding = UDim.new(0, 8)
list.Parent = frame

for _, info in tabs do
local button = Instance.new("ImageButton")
button.Name = info.name .. "Tab"
button.Image = info.icon
button.BackgroundTransparency = 1
button.Size = UDim2.new(0, 48, 0, 48)
button.Parent = frame

button.MouseButton1Click:Connect(function()
self:_activateTab(info.name)
end)
self._tabs[info.name] = button
end
end

function ControlPanel:_buildSearch()
local searchContainer = Instance.new("Frame")
searchContainer.Name = "SearchContainer"
searchContainer.Size = UDim2.new(0, 320, 0, 32)
searchContainer.Position = UDim2.new(0, 24, 0, 84)
searchContainer.BackgroundTransparency = 0.3
searchContainer.BackgroundColor3 = Color3.fromRGB(12, 12, 16)
searchContainer.Parent = self._screenGui

local textBox = Instance.new("TextBox")
textBox.Name = "SearchBox"
textBox.PlaceholderText = "Поиск параметров..."
textBox.Size = UDim2.fromScale(1, 1)
textBox.BackgroundTransparency = 1
textBox.Font = Enum.Font.Gotham
textBox.TextColor3 = Color3.new(1, 1, 1)
textBox.TextXAlignment = Enum.TextXAlignment.Left
textBox.Parent = searchContainer

textBox:GetPropertyChangedSignal("Text"):Connect(function()
self._filterSignal:Fire(textBox.Text)
end)

self._searchBox = textBox
end

function ControlPanel:_activateTab(name: string)
for tabName, button in self._tabs do
local targetTransparency = tabName == name and 0 or 0.5
local tween = TweenService:Create(
button,
TweenInfo.new(0.2, Enum.EasingStyle.Quad, Enum.EasingDirection.Out),
{ ImageTransparency = targetTransparency }
)
tween:Play()
self._animations[tabName] = tween
end
end

function ControlPanel:Start()
self:_activateTab("Movement")
end

function ControlPanel:OnFilterChanged()
return self._filterSignal.Event
end

function ControlPanel:Destroy()
for _, tween in self._animations do
tween:Cancel()
end
self._filterSignal:Destroy()
self._screenGui:Destroy()
end

return ControlPanel
