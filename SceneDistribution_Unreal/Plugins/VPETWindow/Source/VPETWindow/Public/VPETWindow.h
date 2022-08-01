// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

// Do note: VPETWindow - or VPET Helper - was redone from scratch for VPET2 modifications
// New plugin template derives from UE4.27
// It might not be backwards compatible
// For prior versions compatibility, check former source code (relative to original VPET, prior to 2022)

#pragma once

// FSelectionIterator
#include "Engine/Selection.h"
// TActorIterator
#include "EngineUtils.h"
// UEditorLevelLibrary
#include "EditorLevelLibrary.h"
// UEditorAssetLibrary
#include "EditorAssetLibrary.h"
// USceneCaptureComponent2D
#include "Components/SceneCaptureComponent2D.h"
// ASceneCapture2D
#include "Engine/SceneCapture2D.h"
// AStaticMeshActor
#include "Engine/StaticMeshActor.h"
// ADirectionalLight
#include "Engine/DirectionalLight.h"
// UTextureRenderTarget2D
#include "Engine/TextureRenderTarget2D.h"
// IAssetTools::CreateAsset
//#include "IAssetTools.h"
// UTextureRenderTargetFactoryNew
#include "Factories/TextureRenderTargetFactoryNew.h"
// FImageUtils::ExportRenderTarget2DAsPNG
#include "ImageUtils.h"
// FMessageDialog
#include "Misc/MessageDialog.h"
// GameSourceDir
#include "Misc/Paths.h"


#include "CoreMinimal.h"
#include "Modules/ModuleManager.h"

class FToolBarBuilder;
class FMenuBuilder;

class FVPETWindowModule : public IModuleInterface
{
public:

	/** IModuleInterface implementation */
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;
	
	/** This function will be bound to Command. */
	void PluginButtonClicked();
	
private:

	void RegisterMenus();

	TSharedRef<class SDockTab> OnSpawnPluginTab(const class FSpawnTabArgs& SpawnTabArgs);


private:
	TSharedPtr<class FUICommandList> PluginCommands;
};
