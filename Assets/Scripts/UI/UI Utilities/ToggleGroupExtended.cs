// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using System.Collections.Generic;

public class ToggleGroupExtended : UnityEngine.UI.ToggleGroup
{   
    public List<UnityEngine.UI.Toggle> GetToggleGroup()
    {
        return m_Toggles;
    }
}
