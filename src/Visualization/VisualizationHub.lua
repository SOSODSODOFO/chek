--!strict

local TweenService = game:GetService("TweenService")

export type Config = {
indicatorRadius: number?,
colorPalette: { [string]: Color3 }?,
distanceFormat: string?,
}

export type InitContext = {
themeProvider: any?,
}

local DEFAULT_COLORS = {
healthHigh = Color3.fromRGB(68, 230, 92),
healthLow = Color3.fromRGB(230, 83, 68),
priority = Color3.fromRGB(68, 160, 230),
}

local VisualizationHub = {}
VisualizationHub.__index = VisualizationHub

function VisualizationHub.new(config: Config?)
local self = setmetatable({}, VisualizationHub)
self._config = config or {}
self._colorPalette = self._config.colorPalette or DEFAULT_COLORS
self._indicatorRadius = self._config.indicatorRadius or 6
self._distanceFormat = self._config.distanceFormat or "%0.1fm"
self._objects = {}
self._skeletons = {}
self._highlightFolder = Instance.new("Folder")
self._highlightFolder.Name = "ChekHighlights"
local camera = workspace.CurrentCamera
if camera then
self._highlightFolder.Parent = camera
end
self._themeProvider = nil
return self
end

function VisualizationHub:Init(context: InitContext?)
self._themeProvider = context and context.themeProvider or nil
end

function VisualizationHub:Start()
-- Lazy initialize when data is received from analytics
end

function VisualizationHub:_createIndicator(part: BasePart)
local adornment = Instance.new("SphereHandleAdornment")
adornment.Name = "PriorityIndicator"
adornment.Radius = self._indicatorRadius
adornment.Color3 = self._colorPalette.priority
adornment.AlwaysOnTop = true
adornment.ZIndex = 0
adornment.Parent = self._highlightFolder
adornment.Adornee = part
return adornment
end

function VisualizationHub:Highlight(part: BasePart, priority: number)
local indicator = self._objects[part]
if not indicator then
indicator = self:_createIndicator(part)
self._objects[part] = indicator
end
local goal = { Radius = self._indicatorRadius + priority }
local tween = TweenService:Create(indicator, TweenInfo.new(0.35, Enum.EasingStyle.Sine, Enum.EasingDirection.Out), goal)
tween:Play()
end

function VisualizationHub:RenderDistance(part: BasePart, distance: number)
if not part:FindFirstChild("ChekDistanceBillboard") then
local billboard = Instance.new("BillboardGui")
billboard.Name = "ChekDistanceBillboard"
billboard.Size = UDim2.new(0, 120, 0, 32)
billboard.StudsOffsetWorldSpace = Vector3.new(0, 2.5, 0)
billboard.AlwaysOnTop = true
billboard.Parent = part

local label = Instance.new("TextLabel")
label.Name = "DistanceLabel"
label.BackgroundTransparency = 1
label.Size = UDim2.fromScale(1, 1)
label.Font = Enum.Font.GothamSemibold
label.TextScaled = true
label.TextColor3 = self._colorPalette.priority
label.TextStrokeTransparency = 0.4
label.Parent = billboard
end

local billboard = part.ChekDistanceBillboard :: BillboardGui
local label = billboard:FindFirstChild("DistanceLabel") :: TextLabel
if label then
label.Text = string.format(self._distanceFormat, distance)
end
end

function VisualizationHub:ApplySkeleton(part: BasePart)
if self._skeletons[part] then
return
end
local highlight = Instance.new("Highlight")
highlight.Name = "ChekSkeleton"
highlight.FillTransparency = 1
highlight.OutlineTransparency = 0
highlight.OutlineColor = self._colorPalette.priority
highlight.Adornee = part.Parent
highlight.Parent = self._highlightFolder
self._skeletons[part] = highlight
end

function VisualizationHub:SetTheme(theme: { [string]: Color3 })
self._colorPalette = theme
for _, indicator in self._objects do
indicator.Color3 = theme.priority or indicator.Color3
end
end

function VisualizationHub:Update(dt: number)
-- Placeholder for time-based visual effects
end

function VisualizationHub:Destroy()
self._highlightFolder:Destroy()
end

return VisualizationHub
