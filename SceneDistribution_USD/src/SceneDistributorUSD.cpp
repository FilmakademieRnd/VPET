/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2020 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.
The SceneDistributiorUSD module was realized in 2019 and 2020 with funding from the EU funded project SAUCE under grant agreement no 780470

The VPET components SceneDistributiorUSD is intended for research and development
purposes only. Commercial use of any kind is not permitted.

There is no support by Filmakademie. Since the SceneDistributiorUSD is available
for free, Filmakademie shall only be liable for intent and gross negligence;
warranty is limited to malice. Scene DistributiorUSD may under no circumstances
be used for racist, sexual or any illegal purposes. In all non-commercial
productions, scientific publications, prototypical non-commercial software tools,
etc. using the SceneDistributiorUSD Filmakademie has to be named as follows:
“VPET-Virtual Production Editing Tool by Filmakademie Baden-Württemberg,
Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the SceneDistributiorUSD in
a commercial surrounding or for commercial purposes, software based on these
components or any part thereof, the company/individual will have to contact
Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
*/

#include "SceneDistributor.h"
#include <iostream>
#include <fstream>


bool file_exist(const char *fileName)
{
	std::ifstream infile(fileName);
	return infile.good();
}

int main(int argc, char *argv[], char *envp[])
{
	if ((argc <= 1))
		std::cout << "No valid filepath. Please entnter a path and a filename e.g. c:\\USD\\kitchen.usda" << std::endl;
	else if (!file_exist(argv[1]))
		std::cout << "File not found." << std::endl;
	else
		VPET::SceneDistributor distributor((argv[1]));

}

