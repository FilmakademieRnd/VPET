// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Framework/Commands/Commands.h"
#include "VPETTexturerStyle.h"

class FVPETTexturerCommands : public TCommands<FVPETTexturerCommands>
{
public:

	FVPETTexturerCommands()
		: TCommands<FVPETTexturerCommands>(TEXT("VPETTexturer"), NSLOCTEXT("Contexts", "VPETTexturer", "VPETTexturer Plugin"), NAME_None, FVPETTexturerStyle::GetStyleSetName())
	{
	}

	// TCommands<> interface
	virtual void RegisterCommands() override;

public:
	TSharedPtr< FUICommandInfo > OpenPluginWindow;
};