// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

// Do note: VPETWindow - or VPET Helper - was redone from scratch for VPET2 modifications
// New plugin template derives from UE4.27
// It might not be backwards compatible
// For prior versions compatibility, check former source code (relative to original VPET, prior to 2022)

#include "VPETWindow.h"
#include "VPETWindowStyle.h"
#include "VPETWindowCommands.h"
#include "Misc/MessageDialog.h"
#include "ToolMenus.h"

static const FName VPETWindowTabName("VPETWindow");

#define LOCTEXT_NAMESPACE "FVPETWindowModule"

void FVPETWindowModule::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module
	
	FVPETWindowStyle::Initialize();
	FVPETWindowStyle::ReloadTextures();

	FVPETWindowCommands::Register();
	
	PluginCommands = MakeShareable(new FUICommandList);

	PluginCommands->MapAction(
		FVPETWindowCommands::Get().PluginAction,
		FExecuteAction::CreateRaw(this, &FVPETWindowModule::PluginButtonClicked),
		FCanExecuteAction());

	UToolMenus::RegisterStartupCallback(FSimpleMulticastDelegate::FDelegate::CreateRaw(this, &FVPETWindowModule::RegisterMenus));

	FGlobalTabmanager::Get()->RegisterNomadTabSpawner(VPETWindowTabName, FOnSpawnTab::CreateRaw(this, &FVPETWindowModule::OnSpawnPluginTab))
		.SetDisplayName(LOCTEXT("FVPETWindowTabTitle", "VPETWindow"))
		.SetMenuType(ETabSpawnerMenuType::Hidden);
}

void FVPETWindowModule::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.

	UToolMenus::UnRegisterStartupCallback(this);

	UToolMenus::UnregisterOwner(this);

	FVPETWindowStyle::Shutdown();

	FVPETWindowCommands::Unregister();
}

TSharedRef<SDockTab> FVPETWindowModule::OnSpawnPluginTab(const FSpawnTabArgs& SpawnTabArgs)
{
	//
	// Functions
	//
	struct Functions
	{
		// Recursive helped for navigating upwards an attachment hierarchy
		static void AddSendChain(AActor* aActor)
		{
			// Register
			aActor->Modify();
			// Add
			if (aActor->Tags.Find("Send") == INDEX_NONE)
				aActor->Tags.Add(FName("Send"));
			// Walk up attachment chain
			AActor* aParActor = aActor->GetAttachParentActor();
			if (aParActor)
				AddSendChain(aParActor);
		}

		// Add "send" tag to selected objects
		static FReply TagAddSend()
		{
			USelection* sActors = GEditor->GetSelectedActors();
			// Register
			GEditor->BeginTransaction(LOCTEXT("AddSendTransactionName", "Add Send"));
			// For each selected actor
			for (FSelectionIterator sIt(*sActors); sIt; ++sIt)
			{
				if (AActor* lActor = Cast<AActor>(*sIt))
				{
					// Register actor in opened transaction (undo/redo)
					lActor->Modify();

					// Add to it and all parents
					AddSendChain(lActor);
				}
			}
			GEditor->EndTransaction();
			return FReply::Handled();
		}

		// Remove "send" tag from selected objects
		static FReply TagCleanSend()
		{
			USelection* sActors = GEditor->GetSelectedActors();
			// Register
			GEditor->BeginTransaction(LOCTEXT("CleanSendTransactionName", "Clean Send"));
			// For each selected actor
			for (FSelectionIterator sIt(*sActors); sIt; ++sIt)
			{
				if (AActor* lActor = Cast<AActor>(*sIt))
				{
					// Register actor in opened transaction (undo/redo)
					lActor->Modify();

					// Clear
					lActor->Tags.Remove("Send");
				}
			}
			GEditor->EndTransaction();
			return FReply::Handled();
		}

		// Select objects with "send" tag
		static FReply TagSelectSend()
		{
			// Register
			GEditor->BeginTransaction(LOCTEXT("SelectSendTransactionName", "Select Send"));
			// Clear selection
			GEditor->SelectNone(false, true);
			UWorld* lWorld = GEditor->GetEditorWorldContext().World();
			if (GEditor->PlayWorld)
				lWorld = GEditor->PlayWorld;
			if (lWorld)
			{
				// Iterate over all actors
				for (TActorIterator<AActor> aIt(lWorld); aIt; ++aIt)
				{
					AActor* lActor = *aIt;
					if (lActor->Tags.Find("Send") != INDEX_NONE)
						GEditor->SelectActor(lActor, true, true);

				}
			}
			GEditor->EndTransaction();
			return FReply::Handled();
		}

		// Add "editable" tag to selected objects
		static FReply TagAddEditable()
		{
			USelection* sActors = GEditor->GetSelectedActors();
			// Register
			GEditor->BeginTransaction(LOCTEXT("AddEditableTransactionName", "Add Editable"));
			// For each selected actor
			for (FSelectionIterator sIt(*sActors); sIt; ++sIt)
			{
				if (AActor* lActor = Cast<AActor>(*sIt))
				{
					// Register actor in opened transaction (undo/redo)
					lActor->Modify();

					// Add
					if (lActor->Tags.Find("Editable") == INDEX_NONE)
						lActor->Tags.Add(FName("Editable"));
				}
			}
			GEditor->EndTransaction();
			return FReply::Handled();
		}

		// Remove "editable" tag from selected objects
		static FReply TagCleanEditable()
		{
			USelection* sActors = GEditor->GetSelectedActors();
			// Register
			GEditor->BeginTransaction(LOCTEXT("CleanEditableTransactionName", "Clean Editable"));
			// For each selected actor
			for (FSelectionIterator sIt(*sActors); sIt; ++sIt)
			{
				if (AActor* lActor = Cast<AActor>(*sIt))
				{
					// Register actor in opened transaction (undo/redo)
					lActor->Modify();

					// Clear
					lActor->Tags.Remove("Editable");
				}
			}
			GEditor->EndTransaction();
			return FReply::Handled();
		}

		// Select objects with "editable" tag
		static FReply TagSelectEditable()
		{
			// Register
			GEditor->BeginTransaction(LOCTEXT("SelectEditableTransactionName", "Select Editable"));
			// Clear selection
			GEditor->SelectNone(false, true);
			UWorld* lWorld = GEditor->GetEditorWorldContext().World();
			if (GEditor->PlayWorld)
				lWorld = GEditor->PlayWorld;
			if (lWorld)
			{
				// Iterate over all actors
				for (TActorIterator<AActor> aIt(lWorld); aIt; ++aIt)
				{
					AActor* lActor = *aIt;
					if (lActor->Tags.Find("Editable") != INDEX_NONE)
						GEditor->SelectActor(lActor, true, true);

				}
			}
			GEditor->EndTransaction();
			return FReply::Handled();
		}

		// Hide selected objects (in game)
		static FReply VisHideSelected()
		{
			USelection* sActors = GEditor->GetSelectedActors();
			// Register
			GEditor->BeginTransaction(LOCTEXT("HideSelectedTransactionName", "Hide Selected"));
			// For each selected actor
			for (FSelectionIterator sIt(*sActors); sIt; ++sIt)
			{
				if (AActor* lActor = Cast<AActor>(*sIt))
				{
					// Register actor in opened transaction (undo/redo)
					lActor->Modify();

					// Hide
					lActor->SetActorHiddenInGame(true);
				}
			}
			GEditor->EndTransaction();
			return FReply::Handled();
		}

		// Show selected objects (in game)
		static FReply VisShowSelected()
		{
			USelection* sActors = GEditor->GetSelectedActors();
			// Register
			GEditor->BeginTransaction(LOCTEXT("ShowSelectedTransactionName", "Show Selected"));
			// For each selected actor
			for (FSelectionIterator sIt(*sActors); sIt; ++sIt)
			{
				if (AActor* lActor = Cast<AActor>(*sIt))
				{
					// Register actor in opened transaction (undo/redo)
					lActor->Modify();

					// Show
					lActor->SetActorHiddenInGame(false);
				}
			}
			GEditor->EndTransaction();
			return FReply::Handled();
		}

		// Texturer
		// Generates a texture file based on the materials of all objects marked to be send
		static FReply VisTexRun()
		{
			// Settings:
			// Set location of created pre-encoded texture files
			FString texLocation = FPaths::GameSourceDir() + TEXT("../Intermediate/VPET/");
			// Prefix of texture file names
			FString texPrefix = TEXT("VPETtex_");

			//
			// STEP 0
			// 
			// Confirm operation
			FText levelNameText = FText::FromString(UEditorLevelLibrary::GetEditorWorld()->GetName());
			// Can I check if the level has been modified or not.
			FMessageDialog msgDiag;
			FText msgText = FText::Format(LOCTEXT("ExampleFText", "Map {0} will be reloaded. \nUnsaved changes will be discarded."), levelNameText);

			// Stops if cancelled
			if (msgDiag.Open(EAppMsgType::OkCancel, msgText) == EAppReturnType::Cancel)
				return FReply::Handled();

			//
			// STEP 1
			// 
			// Pick all actors that have the edit select tab
			// Add a list of materials
			TArray<UMaterialInterface*> matList;
			UWorld* lWorld = GEditor->GetEditorWorldContext().World();
			if (GEditor->PlayWorld)
				lWorld = GEditor->PlayWorld;
			if (lWorld)
			{
				// Iterate over all actors
				for (TActorIterator<AActor> aIt(lWorld); aIt; ++aIt)
				{
					AActor* lActor = *aIt;
					if (lActor->Tags.Find("Send") != INDEX_NONE)
					{
						// Check if static mesh
						if (lActor->GetClass()->GetName() == "StaticMeshActor")
						{
							TArray<UStaticMeshComponent*> staticMeshComponents;
							lActor->GetComponents<UStaticMeshComponent>(staticMeshComponents);
							// Test if got mesh
							if (staticMeshComponents.Num() == 0) {
								UE_LOG(LogTemp, Warning, TEXT("No Mesh Comp"));
								return FReply::Unhandled();
							}
							// Grab only the first
							UStaticMeshComponent* staticMeshComponent = staticMeshComponents[0];
							UMaterialInterface* thisMaterial = staticMeshComponent->GetMaterial(0);
							int i = matList.AddUnique(thisMaterial);
							if (i == matList.Num() - 1)
								UE_LOG(LogTemp, Warning, TEXT("Added unique material %s"), *thisMaterial->GetName());

						}
					}
				}
			}

			//
			// STEP 2
			//
			// Open a new level
			FString srcLevelName = UEditorLevelLibrary::GetEditorWorld()->GetPathName();
			UE_LOG(LogTemp, Warning, TEXT("Source level: %s"), *srcLevelName);

			// Start fresh
			FString bufferDir("/Game/VPET/TextureCreation/");
			FString levelName("TextureSample.TextureSample");

			if (UEditorAssetLibrary::DoesDirectoryExist(bufferDir))
				UEditorAssetLibrary::DeleteDirectory(bufferDir);

			UEditorLevelLibrary::NewLevel(bufferDir + levelName);

			//
			// STEP 3
			//
			// Populate level
			// Make Scene Capture 2D
			FVector loc(0, 0, 50);
			FRotator rot(-90, 0, 0);
			ASceneCapture2D* cap = (ASceneCapture2D*)UEditorLevelLibrary::SpawnActorFromClass(ASceneCapture2D::StaticClass(), loc, rot);
			USceneCaptureComponent2D* cap2D;
			if (cap)
			{
				cap->SetActorLabel(TEXT("TextureCapture2DLabel"));
				cap->Rename(TEXT("TextureCapture2DName"));
				cap2D = cap->GetCaptureComponent2D();
				cap2D->bCaptureEveryFrame = false;
				cap2D->bCaptureOnMovement = false;
			}
			else
				return FReply::Unhandled();


			// Make Plane
			loc = FVector(0, 0, 0);
			rot = FRotator(0, 90, 0);
			UObject* planeObj = UEditorAssetLibrary::LoadAsset(TEXT("/Engine/BasicShapes/Plane.Plane"));
			AStaticMeshActor* plane = (AStaticMeshActor*)UEditorLevelLibrary::SpawnActorFromObject(planeObj, loc, rot);
			UStaticMeshComponent* pla2D;
			if (plane)
			{
				plane->SetMobility(EComponentMobility::Movable);
				plane->SetActorRelativeScale3D(FVector(1, -1, 1));
				plane->SetActorLabel(TEXT("TexturePlane"));
				plane->Rename(TEXT("TexturePlane"));

				pla2D = plane->GetStaticMeshComponent();
			}
			else
				return FReply::Unhandled();

			// Make totally flat light with a directional light
			loc = FVector(0, 200, 0);
			rot = FRotator(-90, 0, 0);
			ADirectionalLight* ligt = (ADirectionalLight*)UEditorLevelLibrary::SpawnActorFromClass(ADirectionalLight::StaticClass(), loc, rot);
			if (ligt)
			{
				ligt->SetBrightness(3);

				ligt->SetActorLabel(TEXT("TextureLight"));
				ligt->Rename(TEXT("TextureLight"));
			}

			// Make render target - can be transient
			UTextureRenderTargetFactoryNew* NewFactory = NewObject<UTextureRenderTargetFactoryNew>();
			UTextureRenderTarget2D* ScratchRenderTarget256;

			NewFactory->Width = 256;
			NewFactory->Height = 256;
			UObject* NewObj = NewFactory->FactoryCreateNew(UTextureRenderTarget2D::StaticClass(), GetTransientPackage(), NAME_None, RF_Transient, NULL, GWarn);
			ScratchRenderTarget256 = CastChecked<UTextureRenderTarget2D>(NewObj);

			TArray<FString> kFileList;

			if (ScratchRenderTarget256)
			{
				UE_LOG(LogTemp, Warning, TEXT("Source level: %s"), *ScratchRenderTarget256->GetPathName());

				ScratchRenderTarget256->RenderTargetFormat = ETextureRenderTargetFormat::RTF_RGBA8_SRGB;

				cap2D->TextureTarget = ScratchRenderTarget256;
				for (size_t i = 0; i < matList.Num(); i++)
				{
					FString matName = matList[i]->GetName();

					UE_LOG(LogTemp, Warning, TEXT("Material %d: %s"), i, *matName);

					// set material
					pla2D->SetMaterial(0, matList[i]);

					// capture
					cap2D->CaptureScene();

					// now push it
					FString pathOut = texLocation + texPrefix + matName + ".png";

					kFileList.Add(pathOut);

					FArchive* const FileAr = IFileManager::Get().CreateFileWriter(*pathOut);// , FILEWRITE_EvenIfReadOnly);
					if (FileAr)
					{
						//ExportRenderTarget2DAsHDR
						FImageUtils::ExportRenderTarget2DAsPNG(ScratchRenderTarget256, *FileAr);
						FileAr->Close();
					}
				}
			}

			//
			// STEP 4
			//
			// Encode to ASTC
			for (size_t i = 0; i < kFileList.Num(); i++)
			{
				UE_LOG(LogTemp, Warning, TEXT("File %d: %s"), i, *kFileList[i]);

				FString InputFilePath = kFileList[i];
				FString OutputFilePath = InputFilePath.LeftChop(3) + "astc";
				FString CompressionParameters = TEXT("6x6 -medium");

				// Compress PNG file to ASTC (using the reference astcenc.exe from ARM)
				FString Params = FString::Printf(TEXT("-cs \"%s\" \"%s\" %s"),
					*InputFilePath,
					*OutputFilePath,
					*CompressionParameters
				);

				// prepare compressor
				FString CompressorPath(FPaths::EngineDir() + TEXT("Binaries/ThirdParty/ARM/Win64/astcenc-sse4.1.exe"));

				FProcHandle Proc = FPlatformProcess::CreateProc(*CompressorPath, *Params, true, false, false, NULL, -1, NULL, NULL);

				// Failed to start the compressor process
				if (!Proc.IsValid())
				{
					UE_LOG(LogTemp, Error, TEXT("Failed to start astcenc for compressing images (%s)"), *CompressorPath);
					return FReply::Unhandled();
				}

				// Wait for the process to complete
				int ReturnCode;
				while (!FPlatformProcess::GetProcReturnCode(Proc, &ReturnCode))
					FPlatformProcess::Sleep(0.01f);

				// Check status - unsure if this works
				if (ReturnCode != 0)
				{
					UE_LOG(LogTemp, Error, TEXT("ASTC encoder failed with return code %d. Leaving '%s' for testing."), ReturnCode, *InputFilePath);
					return FReply::Unhandled();
				}

				// Success
				UE_LOG(LogTemp, Display, TEXT("ASTC encoder succeeded: %s"), *OutputFilePath);
			}

			//
			// STEP 5
			//
			// Reopen level
			UE_LOG(LogTemp, Warning, TEXT("Attempt to open level: %s"), *srcLevelName);

			UEditorLevelLibrary::LoadLevel(srcLevelName);


			//
			// STEP 6
			//
			// Cleanup
			// Local folder and temporary level
			if (UEditorAssetLibrary::DoesDirectoryExist(bufferDir))
				UEditorAssetLibrary::DeleteDirectory(bufferDir);
			// External files
			for (size_t i = 0; i < kFileList.Num(); i++)
				IFileManager::Get().Delete(*kFileList[i]);


			return FReply::Handled();
		}

	};


	//
	// UI definition
	//
	return SNew(SDockTab)
		.TabRole(ETabRole::NomadTab)
		[
			SNew(SVerticalBox)
			+ SVerticalBox::Slot()
		.AutoHeight()
		.HAlign(HAlign_Center)
		.Padding(6)
		[
			SNew(STextBlock)
			.AutoWrapText(true)
		.Text(LOCTEXT("WidgetTitleText", "VPET Auxiliary Tools"))
		]
	+ SVerticalBox::Slot()
		.HAlign(HAlign_Center)
		.AutoHeight()
		.Padding(0)
		[
			SNew(STextBlock)
			.Text(LOCTEXT("WidgetEmptyText", ""))
		]
	+ SVerticalBox::Slot()
		.HAlign(HAlign_Center)
		.AutoHeight()
		.Padding(6)
		[
			SNew(STextBlock)
			.AutoWrapText(true)
		.Text(LOCTEXT("WidgetTagText", "Tag control"))
		]
	+ SVerticalBox::Slot()
		.HAlign(HAlign_Center)
		.AutoHeight()
		.Padding(4)
		[
			SNew(SHorizontalBox)
			+ SHorizontalBox::Slot()
		.VAlign(VAlign_Bottom)
		.AutoWidth()
		.Padding(4)
		[
			SNew(STextBlock)
			.AutoWrapText(true)
		.Text(LOCTEXT("WidgetSendText", "      Send Tag"))
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("SendAddButtonLabel", "Add"))
		.OnClicked_Static(&Functions::TagAddSend)
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("SendClearButtonLabel", "Clear"))
		.OnClicked_Static(&Functions::TagCleanSend)
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("SendSelectButtonLabel", "Select"))
		.OnClicked_Static(&Functions::TagSelectSend)
		]
		]
	+ SVerticalBox::Slot()
		.HAlign(HAlign_Center)
		.AutoHeight()
		.Padding(4)
		[
			SNew(SHorizontalBox)
			+ SHorizontalBox::Slot()
		.Padding(4)
		.AutoWidth()
		[
			SNew(STextBlock)
			.AutoWrapText(true)
		.Text(LOCTEXT("WidgetEditableText", "Editable Tag"))
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("EditableAddButtonLabel", "Add"))
		.OnClicked_Static(&Functions::TagAddEditable)
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("EditableClearButtonLabel", "Clear"))
		.OnClicked_Static(&Functions::TagCleanEditable)
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("EditableSelectButtonLabel", "Select"))
		.OnClicked_Static(&Functions::TagSelectEditable)
		]
		]
	+ SVerticalBox::Slot()
		.HAlign(HAlign_Center)
		.AutoHeight()
		.Padding(0)
		[
			SNew(STextBlock)
			.Text(LOCTEXT("WidgetEmptyText", ""))
		]
	+ SVerticalBox::Slot()
		.HAlign(HAlign_Center)
		.AutoHeight()
		.Padding(6)
		[
			SNew(STextBlock)
			.AutoWrapText(true)
		.Text(LOCTEXT("WidgetTexPrepText", "Texture preparation"))
		]
	+ SVerticalBox::Slot()
		.HAlign(HAlign_Center)
		.AutoHeight()
		.Padding(4)
		[
			SNew(SHorizontalBox)
			+ SHorizontalBox::Slot()
		.AutoWidth()
		.Padding(4)
		[
			SNew(STextBlock)
			.AutoWrapText(true)
		.Text(LOCTEXT("WidgetTexDebugText", "Set up materials"))
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("VisTexRunLabel", "Run"))
		.OnClicked_Static(&Functions::VisTexRun)
		]
		]
	+ SVerticalBox::Slot()
		.HAlign(HAlign_Center)
		.AutoHeight()
		.Padding(6)
		[
			SNew(STextBlock)
			.AutoWrapText(true)
		.Text(LOCTEXT("WidgetRenderingText", "Unreal rendering"))
		]
	+ SVerticalBox::Slot()
		.HAlign(HAlign_Center)
		.AutoHeight()
		.Padding(4)
		[
			SNew(SHorizontalBox)
			+ SHorizontalBox::Slot()
		.AutoWidth()
		.Padding(4)
		[
			SNew(STextBlock)
			.AutoWrapText(true)
		.Text(LOCTEXT("WidgetVisibilityText", "Visibility"))
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("VisHideButtonLabel", "Hide"))
		.OnClicked_Static(&Functions::VisHideSelected)
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("VisShowButtonLabel", "Show"))
		.OnClicked_Static(&Functions::VisShowSelected)
		]
		]
		];

}

void FVPETWindowModule::PluginButtonClicked()
{
	// Open UI layout
	FGlobalTabmanager::Get()->TryInvokeTab(VPETWindowTabName);
}

void FVPETWindowModule::RegisterMenus()
{
	// Owner will be used for cleanup in call to UToolMenus::UnregisterOwner
	FToolMenuOwnerScoped OwnerScoped(this);

	{
		UToolMenu* Menu = UToolMenus::Get()->ExtendMenu("LevelEditor.MainMenu.Window");
		{
			FToolMenuSection& Section = Menu->FindOrAddSection("WindowLayout");
			Section.AddMenuEntryWithCommandList(FVPETWindowCommands::Get().PluginAction, PluginCommands);
		}
	}

	{
		UToolMenu* ToolbarMenu = UToolMenus::Get()->ExtendMenu("LevelEditor.LevelEditorToolBar");
		{
			FToolMenuSection& Section = ToolbarMenu->FindOrAddSection("Settings");
			{
				FToolMenuEntry& Entry = Section.AddEntry(FToolMenuEntry::InitToolBarButton(FVPETWindowCommands::Get().PluginAction));
				Entry.SetCommandList(PluginCommands);
			}
		}
	}
}

#undef LOCTEXT_NAMESPACE
	
IMPLEMENT_MODULE(FVPETWindowModule, VPETWindow)