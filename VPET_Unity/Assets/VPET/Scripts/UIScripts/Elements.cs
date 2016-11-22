/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
using UnityEngine;
using UnityEngine.UI;
using System;

namespace vpet
{
	public class Elements
	{
		static public IMenuButton MenuButton()
		{
			GameObject refObjectPrefab = Resources.Load<GameObject>("VPET/Prefabs/ButtonTemplate");
			GameObject newButton = GameObject.Instantiate(refObjectPrefab);
			newButton.name = refObjectPrefab.name;
			newButton.GetComponent<RectTransform>().sizeDelta = UI.SpriteSize;
			MenuButton button = newButton.AddComponent<MenuButton>();
			button.interactable = false;
			return (IMenuButton)button;
		}
	
		static public IMenuButton MenuButtonToggle()
		{
			GameObject refObjectPrefab = Resources.Load<GameObject>("VPET/Prefabs/ButtonTemplate");
			GameObject newButton = GameObject.Instantiate(refObjectPrefab);
			newButton.name = refObjectPrefab.name;
			newButton.GetComponent<RectTransform>().sizeDelta = UI.SpriteSize;
			return (IMenuButton)newButton.AddComponent<MenuButtonToggle>();
		}

		static public IMenuButton MenuButtonList()
		{
			GameObject refObjectPrefab = Resources.Load<GameObject>("VPET/Prefabs/ButtonTemplate");
			GameObject newButton = GameObject.Instantiate(refObjectPrefab);
			newButton.name = refObjectPrefab.name;
			newButton.GetComponent<RectTransform>().sizeDelta = UI.SpriteSize;
			return (IMenuButton)newButton.AddComponent<MenuButtonList>();
		}

	}
}

