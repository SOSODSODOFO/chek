--[[
    Chek.lua - configurable Roblox utility suite
    Features:
      * Movement tools: Fly, Noclip, Anti-Aim, velocity control
      * Combat: Aim Assist, Silent Aim, Trigger Damage, FOV customization, Wallbang
      * Visuals: ESP, Chams, tracers, character preview reflecting toggles
      * UI: Themed tabs, interactive parallax background with particles, color themes

    NOTE: Script is designed for exploit environments that allow getgenv() and UI creation.
    Some functionality may require exploit-specific APIs (e.g., hookmetamethod, mousemoverel).
]]

local services = {
    Players = game:GetService("Players"),
    RunService = game:GetService("RunService"),
    UserInputService = game:GetService("UserInputService"),
    TweenService = game:GetService("TweenService"),
    StarterGui = game:GetService("StarterGui"),
    Workspace = game:GetService("Workspace"),
    Lighting = game:GetService("Lighting"),
}

local LocalPlayer = services.Players.LocalPlayer
local Mouse = LocalPlayer:GetMouse()

local assets = {}
local runtimeConnections = {}
local state = {
    theme = "Dark",
    tab = "Aimbot",
    toggles = {
        Fly = false,
        Noclip = false,
        AimAssist = true,
        SilentAim = false,
        SmoothAim = true,
        Wallbang = true,
        ESP = true,
        Chams = true,
        AntiAim = false,
        Damager = false,
        Tracers = true,
        NameTags = true,
        Boxes = false,
        VelocitySpoof = false,
        HighlightSelf = false,
    },
    values = {
        FlySpeed = 60,
        AimFOV = 180,
        AimSmoothing = 0.25,
        SilentHitChance = 92,
        DamageAmount = 10,
        DamageCooldown = 0.35,
        DamagerFOV = 125,
        AntiAimOffsetY = 8,
        ESPRange = 400,
        ParticleIntensity = 0.5,
        ThemeAccent = Color3.fromRGB(66, 135, 245),
    },
    backgrounds = {},
    figureIndicators = {},
    theming = {
        frames = {},
        labels = {},
        accents = {},
    },
    backgroundStyle = "Aurora",
    currentBackground = nil,
    hooks = {},
    lastDamage = 0,
}

local themes = {
    Dark = {
        background = Color3.fromRGB(14, 18, 26),
        foreground = Color3.fromRGB(230, 233, 241),
        accent = Color3.fromRGB(66, 135, 245),
        particleColor = Color3.fromRGB(120, 180, 255),
    },
    Light = {
        background = Color3.fromRGB(247, 248, 255),
        foreground = Color3.fromRGB(44, 52, 64),
        accent = Color3.fromRGB(245, 99, 66),
        particleColor = Color3.fromRGB(255, 190, 160),
    },
    Midnight = {
        background = Color3.fromRGB(10, 8, 24),
        foreground = Color3.fromRGB(221, 205, 255),
        accent = Color3.fromRGB(139, 92, 246),
        particleColor = Color3.fromRGB(180, 127, 255),
    },
}

local backgroundStyles = {
    Aurora = {
        gradient = ColorSequence.new({
            ColorSequenceKeypoint.new(0, Color3.fromRGB(64, 224, 208)),
            ColorSequenceKeypoint.new(0.5, Color3.fromRGB(65, 105, 225)),
            ColorSequenceKeypoint.new(1, Color3.fromRGB(255, 105, 180))
        }),
        particleImage = "rbxassetid://2842054032",
        parallaxStrength = 0.02,
        particleAlpha = 1,
    },
    Nebula = {
        gradient = ColorSequence.new({
            ColorSequenceKeypoint.new(0, Color3.fromRGB(75, 0, 130)),
            ColorSequenceKeypoint.new(0.5, Color3.fromRGB(138, 43, 226)),
            ColorSequenceKeypoint.new(1, Color3.fromRGB(0, 191, 255))
        }),
        particleImage = "rbxassetid://301603327",
        parallaxStrength = 0.035,
        particleAlpha = 0.8,
    },
    Circuit = {
        gradient = ColorSequence.new({
            ColorSequenceKeypoint.new(0, Color3.fromRGB(0, 0, 0)),
            ColorSequenceKeypoint.new(1, Color3.fromRGB(0, 255, 128))
        }),
        particleImage = "rbxassetid://9150671821",
        parallaxStrength = 0.015,
        particleAlpha = 0.6,
    },
}

local tabs = {
    Aimbot = { icon = "rbxassetid://8569159286", title = "Aimbot" },
    Visuals = { icon = "rbxassetid://8569159045", title = "Visuals" },
    Movement = { icon = "rbxassetid://8569158899", title = "Movement" },
    Misc = { icon = "rbxassetid://8569158821", title = "Misc" },
}

local function blendColor(colorA, colorB)
    return Color3.new(
        (colorA.R + colorB.R) / 2,
        (colorA.G + colorB.G) / 2,
        (colorA.B + colorB.B) / 2
    )
end

local function applyBackgroundStyle()
    local style = backgroundStyles[state.backgroundStyle] or backgroundStyles.Aurora
    state.currentBackground = style

    if assets.accentGradient then
        local themeData = themes[state.theme]
        local keypoints = {}
        for _, key in ipairs(style.gradient.Keypoints) do
            table.insert(keypoints, ColorSequenceKeypoint.new(key.Time, blendColor(key.Value, themeData.accent)))
        end
        assets.accentGradient.Color = ColorSequence.new(keypoints)
    end

    for index = 1, 3 do
        local layer = state.backgrounds["particle" .. index]
        if layer then
            layer.Image = style.particleImage
            layer.ImageTransparency = 1 - math.clamp(state.values.ParticleIntensity * style.particleAlpha, 0, 1)
        end
    end
end

local function updateBackgroundButtons()
    if not assets.backgroundButtons then return end
    local themeData = themes[state.theme]
    for name, button in pairs(assets.backgroundButtons) do
        if button and button.Parent then
            local active = name == state.backgroundStyle
            button.BackgroundColor3 = active and themeData.accent or themeData.background:Lerp(themeData.accent, 0.1)
            button.TextColor3 = themeData.foreground
        end
    end
end

local function destroyConnections()
    for _, conn in ipairs(runtimeConnections) do
        conn:Disconnect()
    end
    table.clear(runtimeConnections)
end

local function addRuntimeConnection(signal, callback)
    local conn = signal:Connect(callback)
    table.insert(runtimeConnections, conn)
    return conn
end

local function applyTheme()
    local themeData = themes[state.theme]
    if not themeData then return end

    state.values.ThemeAccent = themeData.accent

    for _, frame in pairs(state.backgrounds) do
        frame.BackgroundColor3 = themeData.background
    end

    for _, frame in ipairs(state.theming.frames) do
        if frame and frame.Parent then
            frame.BackgroundColor3 = themeData.background
        end
    end

    for _, label in ipairs(state.theming.labels) do
        if label and label.Parent then
            label.TextColor3 = themeData.foreground
        end
    end

    for _, accent in ipairs(state.theming.accents) do
        if accent and accent.Parent then
            if accent:IsA("TextButton") or accent:IsA("Frame") then
                accent.BackgroundColor3 = themeData.accent
            elseif accent:IsA("UIGradient") then
                accent.Color = ColorSequence.new({
                    ColorSequenceKeypoint.new(0, themeData.accent),
                    ColorSequenceKeypoint.new(1, themeData.particleColor)
                })
            elseif accent:IsA("ImageLabel") then
                accent.ImageColor3 = themeData.particleColor
            end
        end
    end

    for _, indicator in pairs(state.figureIndicators) do
        indicator.BackgroundColor3 = themeData.accent
    end

    updateFigureIndicators()
    applyBackgroundStyle()
    updateBackgroundButtons()
end

local function createUI()
    if assets.screenGui then
        assets.screenGui:Destroy()
    end

    local screenGui = Instance.new("ScreenGui")
    screenGui.Name = "ChekSuite"
    screenGui.ResetOnSpawn = false
    screenGui.Parent = LocalPlayer:WaitForChild("PlayerGui")

    assets.screenGui = screenGui

    local mainFrame = Instance.new("Frame")
    mainFrame.Size = UDim2.new(0, 700, 0, 480)
    mainFrame.Position = UDim2.new(0.5, -350, 0.5, -240)
    mainFrame.BackgroundTransparency = 0.05
    mainFrame.BackgroundColor3 = themes[state.theme].background
    mainFrame.Parent = screenGui
    mainFrame.ClipsDescendants = true
    assets.mainFrame = mainFrame
    state.backgrounds.main = mainFrame
    table.insert(state.theming.frames, mainFrame)

    local background = Instance.new("Frame")
    background.Name = "InteractiveBackground"
    background.Size = UDim2.new(1, 200, 1, 200)
    background.Position = UDim2.new(0, -100, 0, -100)
    background.BackgroundColor3 = themes[state.theme].background
    background.BackgroundTransparency = 0.2
    background.Parent = mainFrame
    background.ZIndex = 0
    state.backgrounds.parallax = background
    table.insert(state.theming.frames, background)

    local gradient = Instance.new("UIGradient")
    gradient.Color = ColorSequence.new({
        ColorSequenceKeypoint.new(0, themes[state.theme].accent),
        ColorSequenceKeypoint.new(1, themes[state.theme].particleColor)
    })
    gradient.Rotation = 45
    gradient.Parent = background
    assets.accentGradient = gradient

    local particles = Instance.new("Frame")
    particles.Name = "Particles"
    particles.Size = UDim2.new(1, 0, 1, 0)
    particles.BackgroundTransparency = 1
    particles.Parent = background

    for i = 1, 3 do
        local particleLayer = Instance.new("ImageLabel")
        particleLayer.BackgroundTransparency = 1
        particleLayer.Image = "rbxassetid://280702556" -- sparkle
        particleLayer.ImageColor3 = themes[state.theme].particleColor
        particleLayer.ScaleType = Enum.ScaleType.Fit
        particleLayer.Size = UDim2.new(2, 0, 2, 0)
        particleLayer.ZIndex = i
        particleLayer.Parent = particles
        state.backgrounds["particle" .. i] = particleLayer
        table.insert(state.theming.accents, particleLayer)
    end

    local tabHolder = Instance.new("Frame")
    tabHolder.Name = "TabHolder"
    tabHolder.Size = UDim2.new(1, -20, 0, 60)
    tabHolder.Position = UDim2.new(0, 10, 0, 10)
    tabHolder.BackgroundTransparency = 1
    tabHolder.Parent = mainFrame

    local tabLayout = Instance.new("UIListLayout")
    tabLayout.FillDirection = Enum.FillDirection.Horizontal
    tabLayout.Padding = UDim.new(0, 12)
    tabLayout.HorizontalAlignment = Enum.HorizontalAlignment.Left
    tabLayout.Parent = tabHolder

    assets.tabs = {}
    for tabKey, tabData in pairs(tabs) do
        local tabButton = Instance.new("Frame")
        tabButton.Name = tabKey
        tabButton.Size = UDim2.new(0, 150, 1, 0)
        tabButton.BackgroundTransparency = 0.5
        tabButton.BackgroundColor3 = themes[state.theme].background
        tabButton.Parent = tabHolder
        table.insert(state.theming.frames, tabButton)

        local icon = Instance.new("ImageLabel")
        icon.BackgroundTransparency = 1
        icon.Image = tabData.icon
        icon.Size = UDim2.new(0, 36, 0, 36)
        icon.Position = UDim2.new(0, 12, 0.5, -18)
        icon.Parent = tabButton

        local label = Instance.new("TextLabel")
        label.BackgroundTransparency = 1
        label.Size = UDim2.new(1, -60, 1, 0)
        label.Position = UDim2.new(0, 54, 0, 0)
        label.Text = tabData.title
        label.Font = Enum.Font.GothamSemibold
        label.TextSize = 20
        label.TextColor3 = themes[state.theme].foreground
        label.Parent = tabButton
        table.insert(state.theming.labels, label)

        local button = Instance.new("TextButton")
        button.BackgroundTransparency = 1
        button.Text = ""
        button.Size = UDim2.new(1, 0, 1, 0)
        button.Parent = tabButton

        button.MouseButton1Click:Connect(function()
            state.tab = tabKey
            for _, v in pairs(assets.pages) do
                v.Visible = (v.Name == tabKey)
            end
        end)

        assets.tabs[tabKey] = tabButton
    end

    local pageHolder = Instance.new("Frame")
    pageHolder.Name = "PageHolder"
    pageHolder.Size = UDim2.new(1, -20, 1, -120)
    pageHolder.Position = UDim2.new(0, 10, 0, 80)
    pageHolder.BackgroundTransparency = 1
    pageHolder.Parent = mainFrame

    assets.pages = {}

    local function createScroll(name)
        local frame = Instance.new("ScrollingFrame")
        frame.Name = name
        frame.BackgroundTransparency = 1
        frame.Size = UDim2.new(1, 0, 1, 0)
        frame.CanvasSize = UDim2.new(0, 0, 0, 0)
        frame.ScrollBarThickness = 4
        frame.VerticalScrollBarInset = Enum.ScrollBarInset.ScrollBar
        frame.Parent = pageHolder

        local layout = Instance.new("UIListLayout")
        layout.Padding = UDim.new(0, 10)
        layout.HorizontalAlignment = Enum.HorizontalAlignment.Left
        layout.SortOrder = Enum.SortOrder.LayoutOrder
        layout.Parent = frame

        assets.pages[name] = frame
        return frame
    end

    createScroll("Aimbot")
    createScroll("Visuals")
    createScroll("Movement")
    createScroll("Misc")

    for key, page in pairs(assets.pages) do
        page.Visible = (key == state.tab)
    end

    local preview = Instance.new("Frame")
    preview.Name = "Preview"
    preview.Size = UDim2.new(0, 190, 1, -120)
    preview.Position = UDim2.new(1, -200, 0, 80)
    preview.BackgroundTransparency = 0.2
    preview.BackgroundColor3 = themes[state.theme].background
    preview.Parent = mainFrame
    state.backgrounds.preview = preview
    table.insert(state.theming.frames, preview)

    local figure = Instance.new("ImageLabel")
    figure.Size = UDim2.new(1, -20, 1, -40)
    figure.Position = UDim2.new(0, 10, 0, 20)
    figure.BackgroundTransparency = 1
    figure.Image = "rbxassetid://7801135292" -- bacon hair template
    figure.Parent = preview

    local indicatorHolder = Instance.new("Frame")
    indicatorHolder.BackgroundTransparency = 1
    indicatorHolder.Size = UDim2.new(1, 0, 0, 60)
    indicatorHolder.Position = UDim2.new(0, 0, 1, -60)
    indicatorHolder.Parent = preview

    local indicatorLayout = Instance.new("UIListLayout")
    indicatorLayout.FillDirection = Enum.FillDirection.Horizontal
    indicatorLayout.Padding = UDim.new(0, 5)
    indicatorLayout.Parent = indicatorHolder

    local function addIndicator(name)
        local chip = Instance.new("Frame")
        chip.Size = UDim2.new(0, 60, 1, 0)
        chip.BackgroundColor3 = themes[state.theme].accent
        chip.BackgroundTransparency = 0.4
        chip.Parent = indicatorHolder
        table.insert(state.theming.accents, chip)

        local label = Instance.new("TextLabel")
        label.BackgroundTransparency = 1
        label.Text = name
        label.Font = Enum.Font.GothamBold
        label.TextSize = 12
        label.TextColor3 = themes[state.theme].foreground
        label.Size = UDim2.new(1, 0, 1, 0)
        label.Parent = chip
        table.insert(state.theming.labels, label)

        state.figureIndicators[name] = chip
    end

    for indicatorName in pairs(state.toggles) do
        addIndicator(indicatorName)
    end

    local themeSelector = Instance.new("Frame")
    themeSelector.Name = "ThemeSelector"
    themeSelector.Size = UDim2.new(1, -20, 0, 60)
    themeSelector.Position = UDim2.new(0, 10, 1, -70)
    themeSelector.BackgroundTransparency = 0.3
    themeSelector.BackgroundColor3 = themes[state.theme].background
    themeSelector.Parent = mainFrame
    table.insert(state.theming.frames, themeSelector)

    local themeLayout = Instance.new("UIListLayout")
    themeLayout.FillDirection = Enum.FillDirection.Horizontal
    themeLayout.HorizontalAlignment = Enum.HorizontalAlignment.Center
    themeLayout.Padding = UDim.new(0, 12)
    themeLayout.Parent = themeSelector

    for themeName in pairs(themes) do
        local option = Instance.new("TextButton")
        option.Size = UDim2.new(0, 160, 0, 40)
        option.Text = themeName
        option.Font = Enum.Font.GothamSemibold
        option.TextSize = 18
        option.BackgroundColor3 = themes[state.theme].accent
        option.TextColor3 = themes[state.theme].foreground
        option.Parent = themeSelector
        table.insert(state.theming.accents, option)
        table.insert(state.theming.labels, option)

        option.MouseButton1Click:Connect(function()
            state.theme = themeName
            applyTheme()
        end)
    end

    local backgroundSelector = Instance.new("Frame")
    backgroundSelector.Name = "BackgroundSelector"
    backgroundSelector.Size = UDim2.new(1, -20, 0, 60)
    backgroundSelector.Position = UDim2.new(0, 10, 1, -140)
    backgroundSelector.BackgroundTransparency = 0.3
    backgroundSelector.BackgroundColor3 = themes[state.theme].background
    backgroundSelector.Parent = mainFrame
    table.insert(state.theming.frames, backgroundSelector)

    local backgroundLayout = Instance.new("UIListLayout")
    backgroundLayout.FillDirection = Enum.FillDirection.Horizontal
    backgroundLayout.HorizontalAlignment = Enum.HorizontalAlignment.Center
    backgroundLayout.Padding = UDim.new(0, 12)
    backgroundLayout.Parent = backgroundSelector

    assets.backgroundButtons = {}

    for styleName in pairs(backgroundStyles) do
        local option = Instance.new("TextButton")
        option.Size = UDim2.new(0, 160, 0, 40)
        option.Text = styleName
        option.Font = Enum.Font.GothamSemibold
        option.TextSize = 18
        option.BackgroundColor3 = themes[state.theme].accent
        option.TextColor3 = themes[state.theme].foreground
        option.Parent = backgroundSelector
        option.AutoButtonColor = false
        table.insert(state.theming.labels, option)
        table.insert(state.theming.accents, option)

        assets.backgroundButtons[styleName] = option

        option.MouseButton1Click:Connect(function()
            state.backgroundStyle = styleName
            applyBackgroundStyle()
            updateBackgroundButtons()
        end)
    end

    assets.infoLabel = Instance.new("TextLabel")
    assets.infoLabel.Size = UDim2.new(0, 220, 0, 24)
    assets.infoLabel.Position = UDim2.new(0, 10, 0, 50)
    assets.infoLabel.BackgroundTransparency = 1
    assets.infoLabel.Font = Enum.Font.Gotham
    assets.infoLabel.TextSize = 14
    assets.infoLabel.TextXAlignment = Enum.TextXAlignment.Left
    assets.infoLabel.TextColor3 = themes[state.theme].foreground
    assets.infoLabel.Text = "Chek Suite Loaded"
    assets.infoLabel.Parent = mainFrame
    table.insert(state.theming.labels, assets.infoLabel)

    applyTheme()
end

local function parallaxBackground()
    local background = state.backgrounds.parallax
    if not background then return end

    addRuntimeConnection(services.UserInputService.InputChanged, function(input)
        if input.UserInputType == Enum.UserInputType.MouseMovement then
            local style = state.currentBackground or backgroundStyles[state.backgroundStyle] or backgroundStyles.Aurora
            local strength = (style and style.parallaxStrength) or 0.02
            local mouseX, mouseY = input.Position.X, input.Position.Y
            local targetPos = UDim2.new(0, -100 + mouseX * -strength, 0, -100 + mouseY * -strength)
            services.TweenService:Create(background, TweenInfo.new(0.35, Enum.EasingStyle.Sine, Enum.EasingDirection.Out), {
                Position = targetPos
            }):Play()

            for index = 1, 3 do
                local layer = state.backgrounds["particle" .. index]
                if layer then
                    local offset = index * 20 * (strength / 0.02)
                    local goal = {
                        Position = UDim2.new(0, math.sin(mouseX / 80 + index) * offset, 0, math.cos(mouseY / 80 + index) * offset)
                    }
                    services.TweenService:Create(layer, TweenInfo.new(0.4, Enum.EasingStyle.Sine), goal):Play()
                end
            end
        end
    end)
end

local function updateFigureIndicators()
    for name, chip in pairs(state.figureIndicators) do
        if chip and chip:FindFirstChildOfClass("TextLabel") then
            local label = chip.TextLabel
            local enabled = state.toggles[name]
            chip.BackgroundTransparency = enabled and 0.1 or 0.7
            chip.BackgroundColor3 = enabled and themes[state.theme].accent or Color3.fromRGB(95, 95, 95)
            label.TextColor3 = themes[state.theme].foreground
        end
    end
end

local function createToggle(page, text, key)
    local frame = Instance.new("Frame")
    frame.Size = UDim2.new(1, -20, 0, 40)
    frame.BackgroundTransparency = 0.5
    frame.BackgroundColor3 = themes[state.theme].background
    frame.Parent = page
    table.insert(state.theming.frames, frame)

    local label = Instance.new("TextLabel")
    label.BackgroundTransparency = 1
    label.Text = text
    label.Font = Enum.Font.Gotham
    label.TextSize = 16
    label.TextXAlignment = Enum.TextXAlignment.Left
    label.TextColor3 = themes[state.theme].foreground
    label.Size = UDim2.new(0.7, 0, 1, 0)
    label.Position = UDim2.new(0, 10, 0, 0)
    label.Parent = frame
    table.insert(state.theming.labels, label)

    local button = Instance.new("TextButton")
    button.Size = UDim2.new(0, 60, 0, 26)
    button.Position = UDim2.new(1, -70, 0.5, -13)
    button.BackgroundColor3 = themes[state.theme].accent
    button.TextColor3 = Color3.new(1, 1, 1)
    button.Font = Enum.Font.GothamBold
    button.TextSize = 14
    button.Text = state.toggles[key] and "ON" or "OFF"
    button.Parent = frame
    table.insert(state.theming.accents, button)

    button.MouseButton1Click:Connect(function()
        state.toggles[key] = not state.toggles[key]
        button.Text = state.toggles[key] and "ON" or "OFF"
        updateFigureIndicators()
    end)

    return frame
end

local function createSlider(page, text, key, min, max, decimals)
    decimals = decimals or 0

    local frame = Instance.new("Frame")
    frame.Size = UDim2.new(1, -20, 0, 60)
    frame.BackgroundTransparency = 0.5
    frame.BackgroundColor3 = themes[state.theme].background
    frame.Parent = page
    table.insert(state.theming.frames, frame)

    local label = Instance.new("TextLabel")
    label.BackgroundTransparency = 1
    label.Text = string.format("%s: %0." .. decimals .. "f", text, state.values[key])
    label.Font = Enum.Font.Gotham
    label.TextSize = 16
    label.TextXAlignment = Enum.TextXAlignment.Left
    label.TextColor3 = themes[state.theme].foreground
    label.Size = UDim2.new(1, -20, 0, 24)
    label.Position = UDim2.new(0, 10, 0, 6)
    label.Parent = frame
    table.insert(state.theming.labels, label)

    local sliderBar = Instance.new("Frame")
    sliderBar.Size = UDim2.new(1, -20, 0, 6)
    sliderBar.Position = UDim2.new(0, 10, 0, 36)
    sliderBar.BackgroundColor3 = Color3.fromRGB(80, 80, 80)
    sliderBar.Parent = frame

    local fill = Instance.new("Frame")
    fill.Size = UDim2.new((state.values[key] - min) / (max - min), 0, 1, 0)
    fill.BackgroundColor3 = themes[state.theme].accent
    fill.Parent = sliderBar
    table.insert(state.theming.accents, fill)

    local dragging = false

    sliderBar.InputBegan:Connect(function(input)
        if input.UserInputType == Enum.UserInputType.MouseButton1 then
            dragging = true
        end
    end)

    sliderBar.InputEnded:Connect(function(input)
        if input.UserInputType == Enum.UserInputType.MouseButton1 then
            dragging = false
        end
    end)

    services.UserInputService.InputChanged:Connect(function(input)
        if dragging and input.UserInputType == Enum.UserInputType.MouseMovement then
            local relative = math.clamp((input.Position.X - sliderBar.AbsolutePosition.X) / sliderBar.AbsoluteSize.X, 0, 1)
            local value = min + (max - min) * relative
            state.values[key] = value
            fill.Size = UDim2.new(relative, 0, 1, 0)
            label.Text = string.format("%s: %0." .. decimals .. "f", text, value)
            if key == "ParticleIntensity" then
                applyBackgroundStyle()
            elseif key == "AimSmoothing" and not state.toggles.SmoothAim then
                assets.infoLabel.Text = string.format("Smoothing saved: %.2f", value)
            end
        end
    end)
end

local function populatePages()
    local aimPage = assets.pages.Aimbot
    local visualPage = assets.pages.Visuals
    local movePage = assets.pages.Movement
    local miscPage = assets.pages.Misc

    createToggle(aimPage, "Enable Aim Assist", "AimAssist")
    createToggle(aimPage, "Enable Silent Aim", "SilentAim")
    createToggle(aimPage, "Enable Smooth Aim", "SmoothAim")
    createToggle(aimPage, "Allow Wallbang", "Wallbang")
    createToggle(aimPage, "Enable Damager", "Damager")

    createSlider(aimPage, "Aim FOV", "AimFOV", 10, 360, 0)
    createSlider(aimPage, "Aim Smoothing", "AimSmoothing", 0.05, 1, 2)
    createSlider(aimPage, "Silent Hit Chance", "SilentHitChance", 0, 100, 1)
    createSlider(aimPage, "Damager FOV", "DamagerFOV", 10, 360, 0)
    createSlider(aimPage, "Damage Amount", "DamageAmount", 1, 40, 0)

    createToggle(visualPage, "Enable ESP", "ESP")
    createToggle(visualPage, "Enable Chams", "Chams")
    createToggle(visualPage, "Enable Tracers", "Tracers")
    createToggle(visualPage, "Name Tags", "NameTags")
    createToggle(visualPage, "Highlight Self", "HighlightSelf")

    createSlider(visualPage, "ESP Range", "ESPRange", 50, 1000, 0)
    createSlider(visualPage, "Particle Intensity", "ParticleIntensity", 0.1, 1, 2)

    createToggle(movePage, "Enable Fly", "Fly")
    createToggle(movePage, "Enable Noclip", "Noclip")
    createToggle(movePage, "Enable Anti Aim", "AntiAim")
    createToggle(movePage, "Spoof Velocity", "VelocitySpoof")

    createSlider(movePage, "Fly Speed", "FlySpeed", 20, 200, 0)

    createToggle(miscPage, "Wallbang Visualizer", "Wallbang")
    createSlider(miscPage, "Anti Aim Offset Y", "AntiAimOffsetY", -20, 20, 2)
end

local function getClosestTarget(fov)
    local closestDist = fov or state.values.AimFOV
    local closestPlayer
    local camera = workspace.CurrentCamera
    if not camera then return end

    for _, player in ipairs(services.Players:GetPlayers()) do
        if player ~= LocalPlayer and player.Character and player.Character:FindFirstChild("HumanoidRootPart") then
            local hrp = player.Character.HumanoidRootPart
            local pos, onScreen = camera:WorldToViewportPoint(hrp.Position)
            if onScreen then
                local dist = (Vector2.new(pos.X, pos.Y) - Vector2.new(Mouse.X, Mouse.Y)).Magnitude
                if dist < closestDist then
                    closestDist = dist
                    closestPlayer = player
                end
            end
        end
    end
    return closestPlayer
end

local function buildWallbangWhitelist()
    local whitelist = {}
    for _, player in ipairs(services.Players:GetPlayers()) do
        if player ~= LocalPlayer and player.Character then
            table.insert(whitelist, player.Character)
        end
    end
    return whitelist
end

local function aimAt(target, smoothing)
    local camera = workspace.CurrentCamera
    local hrp = target.Character and target.Character:FindFirstChild("HumanoidRootPart")
    if not camera or not hrp then return end

    local targetPos = camera:WorldToScreenPoint(hrp.Position)
    local mousePos = Vector2.new(Mouse.X, Mouse.Y)
    local direction = Vector2.new(targetPos.X, targetPos.Y) - mousePos

    if typeof(mousemoverel) == "function" then
        mousemoverel(direction.X * smoothing, direction.Y * smoothing)
    end
end

local function applySilentAim()
    local status = string.format("Silent Aim Redirect: %s%%", state.values.SilentHitChance)

    if state.toggles.Wallbang then
        status = status .. " | Wallbang"
        if hookmetamethod and getnamecallmethod and not state.hooks.wallbang then
            local oldNamecall
            oldNamecall = hookmetamethod(game, "__namecall", function(self, ...)
                local method = getnamecallmethod()
                if method == "Raycast" and state.toggles.SilentAim and state.toggles.Wallbang then
                    local args = { ... }
                    local origin, direction, params = args[1], args[2], args[3]
                    if typeof(origin) == "Vector3" and typeof(direction) == "Vector3" then
                        local whitelist = buildWallbangWhitelist()
                        if #whitelist == 0 then
                            return oldNamecall(self, ...)
                        end
                        local override = RaycastParams.new()
                        if typeof(params) == "RaycastParams" then
                            override.CollisionGroup = params.CollisionGroup
                            override.IgnoreWater = params.IgnoreWater
                        end
                        override.FilterType = Enum.RaycastFilterType.Whitelist
                        override.FilterDescendantsInstances = whitelist
                        override.RespectCanCollide = false
                        return oldNamecall(self, origin, direction, override)
                    end
                end
                return oldNamecall(self, ...)
            end)
            state.hooks.wallbang = oldNamecall
        elseif not hookmetamethod then
            status = status .. " (limited)"
        end
    end

    assets.infoLabel.Text = status
end

local function applyDamager(target)
    local now = tick()
    if now - state.lastDamage < state.values.DamageCooldown then return end
    state.lastDamage = now

    local humanoid = target.Character and target.Character:FindFirstChildOfClass("Humanoid")
    if humanoid then
        humanoid:TakeDamage(state.values.DamageAmount)
        assets.infoLabel.Text = string.format("Damaged %s for %d", target.Name, state.values.DamageAmount)
    end
end

local function cleanupESP(player)
    if not assets.espCache then return end
    local cache = assets.espCache[player]
    if not cache then return end

    if cache.highlight then cache.highlight:Destroy() end
    if cache.nameTag then cache.nameTag:Destroy() end
    if cache.box then cache.box:Destroy() end
    if cache.tracer then
        if typeof(cache.tracer.Remove) == "function" then
            cache.tracer:Remove()
        elseif typeof(cache.tracer.Destroy) == "function" then
            cache.tracer:Destroy()
        else
            cache.tracer.Visible = false
        end
    end

    assets.espCache[player] = nil
end

local function updateESP()
    assets.espCache = assets.espCache or {}

    local camera = workspace.CurrentCamera
    if not camera then return end

    if not state.toggles.ESP then
        for player in pairs(assets.espCache) do
            cleanupESP(player)
        end
        if assets.selfHighlight then
            assets.selfHighlight.Enabled = false
        end
        return
    end

    if state.toggles.HighlightSelf then
        local character = LocalPlayer.Character
        if character then
            local themeData = themes[state.theme]
            if not assets.selfHighlight or assets.selfHighlight.Parent ~= character then
                local highlight = Instance.new("Highlight")
                highlight.Name = "ChekSelfHighlight"
                highlight.Parent = character
                assets.selfHighlight = highlight
            end
            assets.selfHighlight.Enabled = true
            assets.selfHighlight.FillTransparency = 1
            assets.selfHighlight.OutlineColor = themeData.accent
            assets.selfHighlight.OutlineTransparency = 0
            assets.selfHighlight.Adornee = character
        end
    elseif assets.selfHighlight then
        assets.selfHighlight.Enabled = false
    end

    local tracerOrigin = Vector2.new(camera.ViewportSize.X / 2, camera.ViewportSize.Y - 40)

    for player, cache in pairs(assets.espCache) do
        if not player or not player.Parent then
            cleanupESP(player)
        elseif cache and cache.nameTag and not cache.nameTag.Parent then
            cleanupESP(player)
        end
    end

    for _, player in ipairs(services.Players:GetPlayers()) do
        if player ~= LocalPlayer then
            local character = player.Character
            local hrp = character and character:FindFirstChild("HumanoidRootPart")
            local head = character and character:FindFirstChild("Head")
            if character and hrp and head then
                local cache = assets.espCache[player]
                if not cache then
                    cache = {}
                    assets.espCache[player] = cache
                end

                local themeData = themes[state.theme]
                local distance = (camera.CFrame.Position - hrp.Position).Magnitude
                local withinRange = distance <= state.values.ESPRange

                if not cache.highlight then
                    local highlight = Instance.new("Highlight")
                    highlight.Name = "ChekHighlight"
                    highlight.FillColor = themeData.accent
                    highlight.OutlineColor = themeData.foreground
                    highlight.Parent = character
                    cache.highlight = highlight
                end

                cache.highlight.Enabled = state.toggles.Chams and withinRange
                cache.highlight.FillColor = themeData.accent
                cache.highlight.OutlineColor = themeData.foreground
                cache.highlight.FillTransparency = state.toggles.Chams and 0.5 or 1
                cache.highlight.Adornee = character

                if state.toggles.NameTags and withinRange then
                    if not cache.nameTag then
                        local billboard = Instance.new("BillboardGui")
                        billboard.Name = "ChekNameTag"
                        billboard.AlwaysOnTop = true
                        billboard.Size = UDim2.new(0, 200, 0, 50)
                        billboard.StudsOffset = Vector3.new(0, 2.5, 0)
                        billboard.Parent = head

                        local text = Instance.new("TextLabel")
                        text.Size = UDim2.new(1, 0, 1, 0)
                        text.BackgroundTransparency = 1
                        text.Font = Enum.Font.GothamBold
                        text.TextStrokeTransparency = 0.5
                        text.TextColor3 = themeData.foreground
                        text.TextScaled = true
                        text.Parent = billboard

                        cache.nameTag = billboard
                        cache.nameText = text
                    end

                    cache.nameTag.Enabled = true
                    cache.nameText.Text = string.format("%s | %dm", player.DisplayName, math.floor(distance))
                    cache.nameText.TextColor3 = themeData.foreground
                elseif cache.nameTag then
                    cache.nameTag.Enabled = false
                end

                if state.toggles.Boxes and withinRange then
                    if not cache.box then
                        local box = Instance.new("BoxHandleAdornment")
                        box.Name = "ChekBox"
                        box.AlwaysOnTop = true
                        box.ZIndex = 10
                        box.Size = Vector3.new(4, 6, 3)
                        box.Transparency = 0.7
                        box.Color3 = themeData.accent
                        box.Adornee = hrp
                        box.Parent = hrp
                        cache.box = box
                    end
                    cache.box.Visible = true
                    cache.box.Color3 = themeData.accent
                    cache.box.Adornee = hrp
                elseif cache.box then
                    cache.box.Visible = false
                end

                if state.toggles.Tracers and withinRange then
                    if Drawing and typeof(Drawing.new) == "function" then
                        if not cache.tracer then
                            cache.tracer = Drawing.new("Line")
                            cache.tracer.Thickness = 1.5
                        end
                        cache.tracer.Visible = true
                        cache.tracer.Color = themeData.accent
                        cache.tracer.Transparency = 1
                        cache.tracer.From = tracerOrigin
                        local screenPos, visible = camera:WorldToViewportPoint(hrp.Position)
                        if visible then
                            cache.tracer.To = Vector2.new(screenPos.X, screenPos.Y)
                        else
                            cache.tracer.Visible = false
                        end
                    end
                elseif cache.tracer then
                    cache.tracer.Visible = false
                end
            else
                cleanupESP(player)
            end
        end
    end
end

local function updateMovement(dt)
    local root = LocalPlayer.Character and LocalPlayer.Character:FindFirstChild("HumanoidRootPart")
    local humanoid = LocalPlayer.Character and LocalPlayer.Character:FindFirstChildOfClass("Humanoid")
    if not root or not humanoid then return end

    if state.toggles.Fly then
        humanoid.PlatformStand = true
        local direction = Vector3.new()
        if services.UserInputService:IsKeyDown(Enum.KeyCode.W) then direction = direction + workspace.CurrentCamera.CFrame.LookVector end
        if services.UserInputService:IsKeyDown(Enum.KeyCode.S) then direction = direction - workspace.CurrentCamera.CFrame.LookVector end
        if services.UserInputService:IsKeyDown(Enum.KeyCode.A) then direction = direction - workspace.CurrentCamera.CFrame.RightVector end
        if services.UserInputService:IsKeyDown(Enum.KeyCode.D) then direction = direction + workspace.CurrentCamera.CFrame.RightVector end
        if services.UserInputService:IsKeyDown(Enum.KeyCode.Space) then direction = direction + Vector3.new(0, 1, 0) end
        if services.UserInputService:IsKeyDown(Enum.KeyCode.LeftShift) then direction = direction - Vector3.new(0, 1, 0) end
        if direction.Magnitude > 0 then
            direction = direction.Unit
            root.Velocity = direction * state.values.FlySpeed
        else
            root.Velocity = Vector3.new(0, 0, 0)
        end
    else
        humanoid.PlatformStand = false
    end

    if state.toggles.Noclip then
        for _, part in ipairs(LocalPlayer.Character:GetDescendants()) do
            if part:IsA("BasePart") then
                part.CanCollide = false
            end
        end
    end

    if state.toggles.AntiAim then
        local offsetY = state.values.AntiAimOffsetY
        root.CFrame = root.CFrame + Vector3.new(0, offsetY, 0)
    end

    if state.toggles.VelocitySpoof then
        root.Velocity = Vector3.new(0, 0, 0)
    end
end

local function renderLoop()
    destroyConnections()

    addRuntimeConnection(services.RunService.RenderStepped, function(dt)
        if state.toggles.AimAssist then
            local target = getClosestTarget(state.values.AimFOV)
            if target and state.toggles.SmoothAim then
                aimAt(target, state.values.AimSmoothing)
            elseif target and not state.toggles.SmoothAim then
                aimAt(target, 1)
            end
        end

        if state.toggles.SilentAim then
            applySilentAim()
        end

        if state.toggles.Damager then
            local damageTarget = getClosestTarget(state.values.DamagerFOV)
            if damageTarget then
                applyDamager(damageTarget)
            end
        end

        updateESP()
        updateMovement(dt)
    end)

    parallaxBackground()
end

local function bindHotkeys()
    addRuntimeConnection(services.UserInputService.InputBegan, function(input, gameProcessed)
        if gameProcessed then return end
        if input.KeyCode == Enum.KeyCode.RightShift then
            assets.mainFrame.Visible = not assets.mainFrame.Visible
        elseif input.KeyCode == Enum.KeyCode.F then
            state.toggles.Fly = not state.toggles.Fly
        elseif input.KeyCode == Enum.KeyCode.N then
            state.toggles.Noclip = not state.toggles.Noclip
        elseif input.KeyCode == Enum.KeyCode.X then
            state.toggles.SilentAim = not state.toggles.SilentAim
        end
        updateFigureIndicators()
    end)
end

local function initialize()
    createUI()
    populatePages()
    updateFigureIndicators()
    renderLoop()
    bindHotkeys()
end

initialize()
