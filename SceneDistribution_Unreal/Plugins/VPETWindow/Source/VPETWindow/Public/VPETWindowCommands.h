// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#include "CoreMinimal.h"
#include "Framework/Commands/Commands.h"
#include "VPETWindowStyle.h"

class FVPETWindowCommands : public TCommands<FVPETWindowCommands>
{
public:

	FVPETWindowCommands()
		: TCommands<FVPETWindowCommands>(TEXT("VPETWindow"), NSLOCTEXT("Contexts", "VPETWindow", "VPETWindow Plugin"), NAME_None, FVPETWindowStyle::GetStyleSetName())
	{
	}

	// TCommands<> interface
	virtual void RegisterCommands() override;

public:
	TSharedPtr< FUICommandInfo > PluginAction;
};
