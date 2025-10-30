-- Auto Buy script for GameSense with budget management and priority purchase system

local globals_realtime = globals.realtime
local globals_curtime = globals.curtime
local globals_maxplayers = globals.maxplayers
local globals_tickcount = globals.tickcount
local globals_tickinterval = globals.tickinterval
local globals_mapname = globals.mapname

local client_set_event_callback = client.set_event_callback
local client_console_cmd = client.exec
local client_userid_to_entindex = client.userid_to_entindex
local client_delay_call = client.delay_call

local ui_new_checkbox = ui.new_checkbox
local ui_new_slider = ui.new_slider
local ui_new_combobox = ui.new_combobox
local ui_new_multiselect = ui.new_multiselect
local ui_set = ui.set
local ui_get = ui.get
local ui_set_visible = ui.set_visible
local ui_new_label = ui.new_label

local entity_get_local_player = entity.get_local_player
local entity_get_prop = entity.get_prop
local entity_get_classname = entity.get_classname

local table_insert = table.insert
local table_sort = table.sort
local table_concat = table.concat

local bit_band = bit.band

local TAB = "MISC"
local SUBTAB = "Auto Buy"

local primary_weapons = {
    {name = "-", command = "", cost = 0, classnames = {}},
    {name = "AK-47 / M4A1", command = "buy ak47; buy m4a1; buy m4a1_silencer; ", cost = 3100, classnames = {"CWeaponAK47", "CWeaponM4A1", "CWeaponM4A1Silencer"}},
    {name = "M4A1-S", command = "buy m4a1_silencer; ", cost = 3100, classnames = {"CWeaponM4A1Silencer"}},
    {name = "M4A4", command = "buy m4a1; ", cost = 3100, classnames = {"CWeaponM4A1"}},
    {name = "AK-47", command = "buy ak47; ", cost = 2700, classnames = {"CWeaponAK47"}},
    {name = "AUG / SG553", command = "buy aug; buy sg556; ", cost = 3300, classnames = {"CWeaponAUG", "CWeaponSG556"}},
    {name = "Famas / Galil", command = "buy famas; buy galilar; ", cost = 2250, classnames = {"CWeaponFamas", "CWeaponGalilAR"}},
    {name = "SSG 08", command = "buy ssg08; ", cost = 1700, classnames = {"CWeaponSSG08"}},
    {name = "AWP", command = "buy awp; ", cost = 4750, classnames = {"CWeaponAWP"}},
    {name = "Auto-Sniper", command = "buy scar20; buy g3sg1; ", cost = 5000, classnames = {"CWeaponSCAR20", "CWeaponG3SG1"}},
    {name = "P90", command = "buy p90; ", cost = 2350, classnames = {"CWeaponP90"}},
    {name = "MP9 / MAC-10", command = "buy mp9; buy mac10; ", cost = 1250, classnames = {"CWeaponMP9", "CWeaponMAC10"}},
    {name = "UMP-45", command = "buy ump45; ", cost = 1200, classnames = {"CWeaponUMP45"}},
    {name = "Nova", command = "buy nova; ", cost = 1050, classnames = {"CWeaponNova"}},
    {name = "XM1014", command = "buy xm1014; ", cost = 2000, classnames = {"CWeaponXM1014"}},
    {name = "MAG-7 / Sawed-Off", command = "buy mag7; buy sawedoff; ", cost = 1300, classnames = {"CWeaponMAG7", "CWeaponSawedoff"}},
    {name = "Negev", command = "buy negev; ", cost = 1700, classnames = {"CWeaponNegev"}}
}

local secondary_weapons = {
    {name = "-", command = "", cost = 0, classnames = {}},
    {name = "P250", command = "buy p250; ", cost = 300, classnames = {"CWeaponP250"}},
    {name = "Five-SeveN / Tec-9 / CZ75", command = "buy fn57; buy tec9; buy cz75a; ", cost = 500, classnames = {"CWeaponFiveSeven", "CWeaponTec9", "CWeaponCZ75a"}},
    {name = "Dual Berettas", command = "buy elite; ", cost = 400, classnames = {"CWeaponElite"}},
    {name = "Desert Eagle / R8", command = "buy deagle; buy revolver; ", cost = 700, classnames = {"CWeaponDEagle", "CWeaponRevolver"}}
}

local grenades = {
    {name = "HE Grenade", command = "buy hegrenade; ", cost = 300, classnames = {"CWeaponHEGrenade"}},
    {name = "Smoke Grenade", command = "buy smokegrenade; ", cost = 300, classnames = {"CWeaponSmokeGrenade"}},
    {name = "Molotov / Incendiary", command = "buy molotov; buy incgrenade; ", cost = 600, classnames = {"CWeaponMolotovGrenade", "CWeaponIncGrenade"}},
    {name = "Flashbang", command = "buy flashbang; ", cost = 200, classnames = {"CWeaponFlashbang"}}
}

local gear = {
    kevlar = {command = "buy vest; ", cost = 650},
    helmet = {command = "buy vesthelm; ", cost = 1000},
    defuse = {command = "buy defuser; ", cost = 400}
}

local function get_names(tbl)
    local result = {}
    for i = 1, #tbl do
        result[i] = tbl[i].name
    end
    return result
end

local buybot_enabled = ui_new_checkbox(TAB, SUBTAB, "Enable Auto Buy")
local reserve_slider = ui_new_slider(TAB, SUBTAB, "Reserve money", 0, 16000, 0, true, "$")
local priority_label = ui_new_label(TAB, SUBTAB, "Purchase priority (1 = highest)")

local primary_enable = ui_new_checkbox(TAB, SUBTAB, "Buy primary weapon")
local primary_select = ui_new_combobox(TAB, SUBTAB, "Primary weapon", get_names(primary_weapons))
local primary_priority = ui_new_slider(TAB, SUBTAB, "Primary priority", 1, 6, 2)

local secondary_enable = ui_new_checkbox(TAB, SUBTAB, "Buy secondary weapon")
local secondary_select = ui_new_combobox(TAB, SUBTAB, "Secondary weapon", get_names(secondary_weapons))
local secondary_priority = ui_new_slider(TAB, SUBTAB, "Secondary priority", 1, 6, 1)

local grenades_enable = ui_new_checkbox(TAB, SUBTAB, "Buy grenades")
local he_checkbox = ui_new_checkbox(TAB, SUBTAB, "HE grenade")
local smoke_checkbox = ui_new_checkbox(TAB, SUBTAB, "Smoke grenade")
local molotov_checkbox = ui_new_checkbox(TAB, SUBTAB, "Molotov / Incendiary")
local flash_checkbox = ui_new_checkbox(TAB, SUBTAB, "Flashbang")
local flash_count_slider = ui_new_slider(TAB, SUBTAB, "Flashbang count", 0, 2, 1)
local grenades_priority = ui_new_slider(TAB, SUBTAB, "Grenade priority", 1, 6, 3)

local armor_enable = ui_new_checkbox(TAB, SUBTAB, "Buy armor")
local kevlar_checkbox = ui_new_checkbox(TAB, SUBTAB, "Kevlar")
local helmet_checkbox = ui_new_checkbox(TAB, SUBTAB, "Helmet")
local armor_priority = ui_new_slider(TAB, SUBTAB, "Armor priority", 1, 6, 4)

local defuse_enable = ui_new_checkbox(TAB, SUBTAB, "Buy defuse kit")
local defuse_priority = ui_new_slider(TAB, SUBTAB, "Defuse kit priority", 1, 6, 5)

local knife_enable = ui_new_checkbox(TAB, SUBTAB, "Equip knife after buying")
local knife_priority = ui_new_slider(TAB, SUBTAB, "Knife equip priority", 1, 6, 6)

local function handle_visibility()
    local enabled = ui_get(buybot_enabled)

    ui_set_visible(reserve_slider, enabled)
    ui_set_visible(priority_label, enabled)

    ui_set_visible(primary_enable, enabled)
    ui_set_visible(primary_select, enabled and ui_get(primary_enable))
    ui_set_visible(primary_priority, enabled and ui_get(primary_enable))

    ui_set_visible(secondary_enable, enabled)
    ui_set_visible(secondary_select, enabled and ui_get(secondary_enable))
    ui_set_visible(secondary_priority, enabled and ui_get(secondary_enable))

    ui_set_visible(grenades_enable, enabled)
    local grenades_visible = enabled and ui_get(grenades_enable)
    ui_set_visible(he_checkbox, grenades_visible)
    ui_set_visible(smoke_checkbox, grenades_visible)
    ui_set_visible(molotov_checkbox, grenades_visible)
    ui_set_visible(flash_checkbox, grenades_visible)
    ui_set_visible(flash_count_slider, grenades_visible and ui_get(flash_checkbox))
    ui_set_visible(grenades_priority, grenades_visible)

    ui_set_visible(armor_enable, enabled)
    local armor_visible = enabled and ui_get(armor_enable)
    ui_set_visible(kevlar_checkbox, armor_visible)
    ui_set_visible(helmet_checkbox, armor_visible)
    ui_set_visible(armor_priority, armor_visible and (ui_get(kevlar_checkbox) or ui_get(helmet_checkbox)))

    ui_set_visible(defuse_enable, enabled)
    ui_set_visible(defuse_priority, enabled and ui_get(defuse_enable))

    ui_set_visible(knife_enable, enabled)
    ui_set_visible(knife_priority, enabled and ui_get(knife_enable))
end

handle_visibility()

local visibility_controls = {
    buybot_enabled,
    primary_enable,
    secondary_enable,
    grenades_enable,
    flash_checkbox,
    armor_enable,
    kevlar_checkbox,
    helmet_checkbox,
    defuse_enable,
    knife_enable
}

for i = 1, #visibility_controls do
    ui.set_callback(visibility_controls[i], handle_visibility)
end

local function find_entry_by_name(tbl, name)
    for i = 1, #tbl do
        if tbl[i].name == name then
            return tbl[i]
        end
    end
    return tbl[1]
end

local function get_inventory_classnames(player)
    local owned = {}
    for i = 0, 63 do
        local weapon = entity_get_prop(player, "m_hMyWeapons", i)
        if weapon ~= nil then
            if weapon ~= -1 then
                weapon = bit_band(weapon, 0xFFF)
                if weapon > 0 then
                    local classname = entity_get_classname(weapon)
                    if classname ~= nil then
                        owned[classname] = true
                    end
                end
            end
        end
    end
    return owned
end

local function has_any_class(owned, classnames)
    if classnames == nil then
        return false
    end
    for i = 1, #classnames do
        if owned[classnames[i]] then
            return true
        end
    end
    return false
end

local function has_flash_count(player)
    local count = 0
    for i = 0, 63 do
        local weapon = entity_get_prop(player, "m_hMyWeapons", i)
        if weapon ~= nil and weapon ~= -1 then
            weapon = bit_band(weapon, 0xFFF)
            if weapon > 0 then
                local classname = entity_get_classname(weapon)
                if classname == "CWeaponFlashbang" then
                    count = count + 1
                end
            end
        end
    end
    return count
end

local function get_money(player)
    return entity_get_prop(player, "m_iAccount") or 0
end

local function has_helmet(player)
    return entity_get_prop(player, "m_bHasHelmet") == 1
end

local function has_kevlar(player)
    return (entity_get_prop(player, "m_ArmorValue") or 0) > 0
end

local function has_defuse(player)
    return entity_get_prop(player, "m_bHasDefuser") == 1
end

local function build_purchase_plan(player)
    local money = get_money(player)
    local reserve = ui_get(reserve_slider)
    local available_budget = money - reserve
    if available_budget <= 0 then
        return nil
    end

    local owned = get_inventory_classnames(player)
    local plan = {}

    if ui_get(primary_enable) then
        local selection = ui_get(primary_select)
        local entry = find_entry_by_name(primary_weapons, selection)
        if entry and entry.cost > 0 and not has_any_class(owned, entry.classnames) then
            table_insert(plan, {priority = ui_get(primary_priority), cost = entry.cost, command = entry.command})
        end
    end

    if ui_get(secondary_enable) then
        local selection = ui_get(secondary_select)
        local entry = find_entry_by_name(secondary_weapons, selection)
        if entry and entry.cost > 0 and not has_any_class(owned, entry.classnames) then
            table_insert(plan, {priority = ui_get(secondary_priority), cost = entry.cost, command = entry.command})
        end
    end

    if ui_get(grenades_enable) then
        if ui_get(he_checkbox) and not has_any_class(owned, grenades[1].classnames) then
            table_insert(plan, {priority = ui_get(grenades_priority), cost = grenades[1].cost, command = grenades[1].command})
        end

        if ui_get(smoke_checkbox) and not has_any_class(owned, grenades[2].classnames) then
            table_insert(plan, {priority = ui_get(grenades_priority), cost = grenades[2].cost, command = grenades[2].command})
        end

        if ui_get(molotov_checkbox) and not has_any_class(owned, grenades[3].classnames) then
            table_insert(plan, {priority = ui_get(grenades_priority), cost = grenades[3].cost, command = grenades[3].command})
        end

        if ui_get(flash_checkbox) then
            local desired_count = ui_get(flash_count_slider)
            local owned_count = has_flash_count(player)
            local missing = desired_count - owned_count
            for i = 1, missing do
                if missing > 0 then
                    table_insert(plan, {priority = ui_get(grenades_priority), cost = grenades[4].cost, command = grenades[4].command})
                end
            end
        end
    end

    if ui_get(armor_enable) then
        local want_kevlar = ui_get(kevlar_checkbox)
        local want_helmet = ui_get(helmet_checkbox)
        if want_helmet and not has_helmet(player) then
            table_insert(plan, {priority = ui_get(armor_priority), cost = gear.helmet.cost, command = gear.helmet.command})
        elseif want_kevlar and not has_kevlar(player) then
            table_insert(plan, {priority = ui_get(armor_priority), cost = gear.kevlar.cost, command = gear.kevlar.command})
        end
    end

    if ui_get(defuse_enable) and not has_defuse(player) then
        table_insert(plan, {priority = ui_get(defuse_priority), cost = gear.defuse.cost, command = gear.defuse.command})
    end

    if ui_get(knife_enable) then
        table_insert(plan, {priority = ui_get(knife_priority), cost = 0, command = "use weapon_knife; "})
    end

    if #plan == 0 then
        return nil
    end

    table_sort(plan, function(a, b)
        if a.priority == b.priority then
            return a.cost > b.cost
        end
        return a.priority < b.priority
    end)

    local commands = {}
    local spent = 0

    for i = 1, #plan do
        local item = plan[i]
        if item.cost == 0 then
            table_insert(commands, item.command)
        elseif spent + item.cost <= available_budget then
            spent = spent + item.cost
            table_insert(commands, item.command)
        end
    end

    if #commands == 0 then
        return nil
    end

    return table_concat(commands)
end

local function execute_buy()
    local local_player = entity_get_local_player()
    if local_player == nil then
        return
    end

    local command = build_purchase_plan(local_player)
    if command ~= nil and #command > 0 then
        client_console_cmd(command)
    end
end

local function on_player_spawn(event)
    local userid = event.userid
    if userid == nil then
        return
    end

    local local_player = entity_get_local_player()
    if local_player == nil then
        return
    end

    if client_userid_to_entindex(userid) ~= local_player then
        return
    end

    if not ui_get(buybot_enabled) then
        return
    end

    client_delay_call(0.2, execute_buy)
end

client_set_event_callback("player_spawn", on_player_spawn)
