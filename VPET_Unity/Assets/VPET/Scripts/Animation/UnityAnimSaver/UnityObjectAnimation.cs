/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

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
using System.Collections;

public class UnityObjectAnimation {

	public UnityCurveContainer[] curves;
	public Transform observeGameObject;
	public string pathName = "";

	public UnityObjectAnimation( string hierarchyPath, Transform observeObj ) {
		pathName = hierarchyPath;
		observeGameObject = observeObj;

		curves = new UnityCurveContainer[10];

		curves [0] = new UnityCurveContainer( "localPosition.x" );
		curves [1] = new UnityCurveContainer( "localPosition.y" );
		curves [2] = new UnityCurveContainer( "localPosition.z" );

		curves [3] = new UnityCurveContainer( "localRotation.x" );
		curves [4] = new UnityCurveContainer( "localRotation.y" );
		curves [5] = new UnityCurveContainer( "localRotation.z" );
		curves [6] = new UnityCurveContainer( "localRotation.w" );


		curves [7] = new UnityCurveContainer( "localScale.x" );
		curves [8] = new UnityCurveContainer( "localScale.y" );
		curves [9] = new UnityCurveContainer( "localScale.z" );
	}

	public void AddFrame ( float time ) {

		curves [0].AddValue (time, observeGameObject.localPosition.x);
		curves [1].AddValue (time, observeGameObject.localPosition.y);
		curves [2].AddValue (time, observeGameObject.localPosition.z);

		curves [3].AddValue (time, observeGameObject.localRotation.x);
		curves [4].AddValue (time, observeGameObject.localRotation.y);
		curves [5].AddValue (time, observeGameObject.localRotation.z);
		curves [6].AddValue (time, observeGameObject.localRotation.w);

		curves [7].AddValue (time, observeGameObject.localScale.x);
		curves [8].AddValue (time, observeGameObject.localScale.y);
		curves [9].AddValue (time, observeGameObject.localScale.z);

	}
}
