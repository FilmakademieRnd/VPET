// Copyright Epic Games, Inc. All Rights Reserved.

#pragma once

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

#include "CoreMinimal.h"
#include "Modules/ModuleManager.h"

class FToolBarBuilder;
class FMenuBuilder;

class FVPETTexturerModule : public IModuleInterface
{
public:

	/** IModuleInterface implementation */
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;
	
	/** This function will be bound to Command (by default it will bring up plugin window) */
	void PluginButtonClicked();
	
private:

	// Vars
	TArray<int> extIntArray;
	TArray<UMaterialInterface*> matList;

	FString sourceLevelName = TEXT("");

	TArray<FString> fileList;

	void RegisterMenus();

	TSharedRef<class SDockTab> OnSpawnPluginTab(const class FSpawnTabArgs& SpawnTabArgs);

private:
	TSharedPtr<class FUICommandList> PluginCommands;
};
