// Copyright Epic Games, Inc. All Rights Reserved.

#include "VPETTexturerCommands.h"

#define LOCTEXT_NAMESPACE "FVPETTexturerModule"

void FVPETTexturerCommands::RegisterCommands()
{
	UI_COMMAND(OpenPluginWindow, "VPETTexturer", "Bring up VPETTexturer window", EUserInterfaceActionType::Button, FInputGesture());
}

#undef LOCTEXT_NAMESPACE
