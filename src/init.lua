--!strict

--[=[
Main entry point for the Chek control interface framework.
This module wires together control, analytics, visualization and UI systems.
It is intended to run inside Roblox as a ModuleScript that can be required
by a LocalScript. All subsystems expose a consistent lifecycle API
(`:Init()`, `:Start()`, `:Destroy()`) and are orchestrated here.
]=]

local RunService = game:GetService("RunService")

local ConfigRegistry = require(script.Parent.Config.ConfigRegistry)
local MovementModule = require(script.Parent.ControlSystems.MovementModule)
local EnvironmentModule = require(script.Parent.ControlSystems.EnvironmentModule)
local AnalyticsHub = require(script.Parent.Analytics.AnalyticsHub)
local VisualizationHub = require(script.Parent.Visualization.VisualizationHub)
local ControlPanel = require(script.Parent.UI.ControlPanel)
local ThemeManager = require(script.Parent.UI.ThemeManager)
local PreviewModule = require(script.Parent.UI.PreviewModule)
local BackgroundEffects = require(script.Parent.UI.BackgroundEffects)
local AutomationSuite = require(script.Parent.Automation.AutomationSuite)
local SystemManager = require(script.Parent.Systems.SystemManager)
local ObjectManager = require(script.Parent.Systems.ObjectManager)
local EnvironmentModifiers = require(script.Parent.Systems.EnvironmentModifiers)
local PerformanceAnalyzer = require(script.Parent.Systems.PerformanceAnalyzer)
local Logger = require(script.Parent.Systems.Logger)

type EnvironmentModifierConfig = {
Atmosphere: {
Density: number?,
Offset: number?,
Color: Color3?,
}?,
FogEnd: number?,
Ambient: Color3?,
}

export type InitConfig = {
profiles: { [string]: ConfigRegistry.ProfileConfig }?,
initialProfile: string?,
movement: MovementModule.Config?,
environment: EnvironmentModule.Config?,
analytics: AnalyticsHub.Config?,
visualization: VisualizationHub.Config?,
theme: ThemeManager.ThemeConfig?,
backgrounds: BackgroundEffects.Config?,
automation: AutomationSuite.Config?,
camera: SystemManager.CameraConfig?,
logging: SystemManager.LoggingConfig?,
environmentModifiers: EnvironmentModifierConfig?,
}

local Framework = {}
Framework.__index = Framework

function Framework.new(config: InitConfig?)
local self = setmetatable({}, Framework)
self._config = config or {}
self._configRegistry = ConfigRegistry.new(self._config.profiles)
self._movement = MovementModule.new(self._config.movement)
self._environment = EnvironmentModule.new(self._config.environment)
self._analytics = AnalyticsHub.new(self._config.analytics)
self._visualization = VisualizationHub.new(self._config.visualization)
self._themeManager = ThemeManager.new(self._config.theme)
self._logger = Logger.new({
enabled = self._config.logging and self._config.logging.enabled,
historySize = self._config.logging and self._config.logging.historySize,
})
self._performanceAnalyzer = PerformanceAnalyzer.new()
self._objectManager = ObjectManager.new()
self._environmentModifiers = EnvironmentModifiers.new()
self._controlPanel = ControlPanel.new({
movement = self._movement,
environment = self._environment,
analytics = self._analytics,
visualization = self._visualization,
themeManager = self._themeManager,
automation = self._automation,
})
self._preview = PreviewModule.new()
self._backgrounds = BackgroundEffects.new(self._config.backgrounds)
self._automation = AutomationSuite.new(self._config.automation)
self._systemManager = SystemManager.new({
camera = self._config.camera,
logging = self._config.logging,
movement = self._movement,
environment = self._environment,
analytics = self._analytics,
visualization = self._visualization,
logger = self._logger,
performanceAnalyzer = self._performanceAnalyzer,
objectManager = self._objectManager,
environmentModifiers = self._environmentModifiers,
})
self._maid = {}

return self
end

function Framework:Init()
self._configRegistry:Init()
self._movement:Init()
self._environment:Init()
self._analytics:Init()
self._visualization:Init({ themeProvider = self._themeManager })
self._themeManager:Init()
self._controlPanel:Init({ themeManager = self._themeManager })
self._preview:Init(self._movement, self._visualization)
self._backgrounds:Init()
self._automation:Init({ movement = self._movement, environment = self._environment })
self._performanceAnalyzer:Init()
if self._config.environmentModifiers then
self._environmentModifiers:Apply(self._config.environmentModifiers)
end
self._systemManager:Init()
end

function Framework:Start()
self._movement:Start()
self._environment:Start()
self._analytics:Start()
self._visualization:Start()
self._themeManager:ApplyTheme(self._config.initialProfile)
self._controlPanel:Start()
self._preview:Start()
self._backgrounds:Start()
self._automation:Start()
self._systemManager:Start()

table.insert(self._maid, RunService.Heartbeat:Connect(function(dt)
self._movement:Update(dt)
self._environment:Update(dt)
self._analytics:Update(dt)
self._visualization:Update(dt)
self._preview:Update(dt)
self._backgrounds:Update(dt)
self._automation:Update(dt)
self._systemManager:Update(dt)
end))
end

function Framework:Destroy()
for _, connection in ipairs(self._maid) do
connection:Disconnect()
end
self._maid = {}

self._systemManager:Destroy()
self._automation:Destroy()
self._backgrounds:Destroy()
self._preview:Destroy()
self._controlPanel:Destroy()
self._themeManager:Destroy()
self._visualization:Destroy()
self._analytics:Destroy()
self._environment:Destroy()
self._movement:Destroy()
self._configRegistry:Destroy()
self._performanceAnalyzer:Destroy()
self._logger = nil
self._objectManager:Destroy()
self._environmentModifiers:Reset()
end

return Framework
