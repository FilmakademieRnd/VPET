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
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; version 2.1 of the License.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html
-----------------------------------------------------------------------------
*/

using UnityEngine;
using System.Collections;

public class drawGrid : MonoBehaviour {

    //public GameObject plane;

    public bool showMain = true;
    public bool showSub = false;

    public int gridSizeX;
    public int gridSizeZ;

    public float smallStep;
    public float largeStep;

    private Material lineMaterial;

    public Color mainColor = new Color(.8f, .8f, .8f, 1f);
    public Color subColor = new Color(.5f, .5f, .5f, 1f);

    void Start()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    void OnPostRender()
    {
        // set the current material
        lineMaterial.SetPass(0);

        GL.Begin(GL.LINES);

        int startX = -gridSizeX / 2;
        int startZ = -gridSizeZ / 2;

        if (showSub)
        {
            GL.Color(subColor);

            //X axis lines
            for (float i = 0; i <= gridSizeZ; i += smallStep)
            {
                GL.Vertex3(startX, 0, startZ + i);
                GL.Vertex3(startX + gridSizeX, 0, startZ + i);
            }

            //Z axis lines
            for (float i = 0; i <= gridSizeX; i += smallStep)
            {
                GL.Vertex3(startX + i, 0, startZ);
                GL.Vertex3(startX + i, 0, startZ + gridSizeZ);
            }
        }

        if (showMain)
        {
            GL.Color(mainColor);

            //Layers

            //X axis lines
            for (float i = 0; i <= gridSizeZ; i += largeStep)
            {
                GL.Vertex3(startX, 0, startZ + i);
                GL.Vertex3(startX + gridSizeX, 0, startZ + i);
            }

            //Z axis lines
            for (float i = 0; i <= gridSizeX; i += largeStep)
            {
                GL.Vertex3(startX + i, 0, startZ);
                GL.Vertex3(startX + i, 0, startZ + gridSizeZ);
            }

        }


        GL.End();
    }
}
