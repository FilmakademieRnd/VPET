// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "VPETWindowCommands.h"

#define LOCTEXT_NAMESPACE "FVPETWindowModule"

void FVPETWindowCommands::RegisterCommands()
{
	UI_COMMAND(PluginAction, "VPETWindow", "Execute VPETWindow action", EUserInterfaceActionType::Button, FInputGesture());
}

#undef LOCTEXT_NAMESPACE
