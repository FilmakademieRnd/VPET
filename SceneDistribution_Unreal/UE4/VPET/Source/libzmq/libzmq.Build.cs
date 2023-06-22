/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tools
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2020 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

The VPET component Unreal Scene Distribution is intended for research and development
purposes only. Commercial use of any kind is not permitted.

There is no support by Filmakademie. Since the Unreal Scene Distribution is available
for free, Filmakademie shall only be liable for intent and gross negligence;
warranty is limited to malice. Scene DistributiorUSD may under no circumstances
be used for racist, sexual or any illegal purposes. In all non-commercial
productions, scientific publications, prototypical non-commercial software tools,
etc. using the Unreal Scene Distribution Filmakademie has to be named as follows:
“VPET-Virtual Production Editing Tool by Filmakademie Baden-Württemberg,
Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the Unreal Scene Distribution in
a commercial surrounding or for commercial purposes, software based on these
components or any part thereof, the company/individual will have to contact
Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
*/


using System.IO;
using UnrealBuildTool;

public class libzmq : ModuleRules
{
    public libzmq(ReadOnlyTargetRules Target) : base(Target)
    {
        //Type = ModuleType.External;
        PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;

        if (Target.Platform == UnrealTargetPlatform.Win64)
        {
            // Include
            // Include files from former plugin - needed a minor adjustment at include files 
            PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "..", "..", "ThirdParty", "libzmq_4.3.1", "include"));
            // Most recent include files - will lead to some deprecated methods warnings, but builds correctly
            //PublicIncludePaths.Add(Path.Combine(ModuleDirectory, "..", "..", "ThirdParty", "libzmq_432", "include"));

            // Library
            PublicAdditionalLibraries.Add(Path.Combine(ModuleDirectory, "..", "..", "ThirdParty", "libzmq_4.3.1", "Windows", "x64", "libzmq-v141-mt-s-4_3_2.lib"));
            // Fresh release lib - doesn't work for the build
            //PublicAdditionalLibraries.Add(Path.Combine(ModuleDirectory, "..", "..", "ThirdParty", "libzmq_432", "win_x64", "libzmq-v141-mt-s-4_3_2.lib"));

            // Add definition for zmq library
            PublicDefinitions.Add("ZMQ_STATIC");
        }

        // Basic core functionality
        PublicDependencyModuleNames.AddRange(new string[] { "Core", });


    }
}