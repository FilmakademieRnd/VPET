// Copyright Epic Games, Inc. All Rights Reserved.

#include "VPETTexturer.h"
#include "VPETTexturerStyle.h"
#include "VPETTexturerCommands.h"
#include "LevelEditor.h"
#include "Widgets/Docking/SDockTab.h"
#include "Widgets/Layout/SBox.h"
#include "Widgets/Text/STextBlock.h"
#include "ToolMenus.h"

static const FName VPETTexturerTabName("VPETTexturer");

FString StringTest = TEXT("Hello");

#define LOCTEXT_NAMESPACE "FVPETTexturerModule"

void FVPETTexturerModule::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module
	
	FVPETTexturerStyle::Initialize();
	FVPETTexturerStyle::ReloadTextures();

	FVPETTexturerCommands::Register();
	
	PluginCommands = MakeShareable(new FUICommandList);

	PluginCommands->MapAction(
		FVPETTexturerCommands::Get().OpenPluginWindow,
		FExecuteAction::CreateRaw(this, &FVPETTexturerModule::PluginButtonClicked),
		FCanExecuteAction());

	UToolMenus::RegisterStartupCallback(FSimpleMulticastDelegate::FDelegate::CreateRaw(this, &FVPETTexturerModule::RegisterMenus));
	
	FGlobalTabmanager::Get()->RegisterNomadTabSpawner(VPETTexturerTabName, FOnSpawnTab::CreateRaw(this, &FVPETTexturerModule::OnSpawnPluginTab))
		.SetDisplayName(LOCTEXT("FVPETTexturerTabTitle", "VPETTexturer"))
		.SetMenuType(ETabSpawnerMenuType::Hidden);
}

void FVPETTexturerModule::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.

	UToolMenus::UnRegisterStartupCallback(this);

	UToolMenus::UnregisterOwner(this);

	FVPETTexturerStyle::Shutdown();

	FVPETTexturerCommands::Unregister();

	FGlobalTabmanager::Get()->UnregisterNomadTabSpawner(VPETTexturerTabName);
}


TSharedRef<SDockTab> FVPETTexturerModule::OnSpawnPluginTab(const FSpawnTabArgs& SpawnTabArgs)
{
	//
	// Functions
	//
	struct Functions
	{
		static FReply VisTexGrabMatList(TArray<UMaterialInterface*>* matList)
		{
			// Pick all actors that have the edit select tab
			// Add a list of materials
			// Print the material names
			
			// Array of name for not adding duplicates 


			// Register
			GEditor->BeginTransaction(LOCTEXT("TexAct1TransactionName", "Tex Act 1"));
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
							int i = matList->AddUnique(thisMaterial);
							if (i == matList->Num() - 1)
								UE_LOG(LogTemp, Warning, TEXT("Added unique material %s"), *thisMaterial->GetName());
				
						}
					}
				}
			}
			GEditor->EndTransaction();
			return FReply::Handled();
		}

		static FReply VisTexAct2(TArray<UMaterialInterface*>* matList)
		{
			// Print material list

			for (size_t i = 0; i < matList->Num(); i++)
			{
				UE_LOG(LogTemp, Warning, TEXT("Material %d: %s"), i, *(*matList)[i]->GetName());
			}

			return FReply::Handled();
		}

		static FReply VisTexNewLevel(FString* kLevelName)
		{

			FString srcLevelName = UEditorLevelLibrary::GetEditorWorld()->GetPathName();
			UE_LOG(LogTemp, Warning, TEXT("Source level: %s"), *srcLevelName);
			(*kLevelName) = srcLevelName;
			UE_LOG(LogTemp, Warning, TEXT("Source level at pointer: %s"), *(*kLevelName));

			// Start fresh
			FString bufferDir("/Game/VPET/TextureCreation/");
			FString levelName("TextureSample.TextureSample");

			if (UEditorAssetLibrary::DoesDirectoryExist(bufferDir))
				UEditorAssetLibrary::DeleteDirectory(bufferDir);

			UEditorLevelLibrary::NewLevel(bufferDir + levelName);

			
			// Print material list

			//for (size_t i = 0; i < matList->Num(); i++)
			//{
			//	UE_LOG(LogTemp, Warning, TEXT("Material %d: %s"), i, *(*matList)[i]->GetName());
			//}

			return FReply::Handled();
		}

		static FReply VisTexAddContent(TArray<UMaterialInterface*>* matList, TArray<FString>* kFileList)
		{
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
			if(plane)
			{
				plane->SetActorRelativeScale3D(FVector(1, -1, 1));
				plane->SetMobility(EComponentMobility::Movable);
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

			// Make render target - can be transient ?
			//UTextureRenderTarget2D rt = UTextureRenderTarget2D();
			//UTextureRenderTarget2D* rt = NewObject<UTextureRenderTarget2D>();
			//if (rt)
			//{ 
			//	UE_LOG(LogTemp, Warning, TEXT("Source level: %s"), *rt->GetPathName());
			//	rt->SizeX = 256;
			//	rt->SizeY = 256;
			//	rt->RenderTargetFormat = ETextureRenderTargetFormat::RTF_RGBA8_SRGB;

			//	// This breaks
			//	//cap2D->TextureTarget = rt;
			//}
			

			// Loading from asset
			//UTextureRenderTarget2D* rt = (UTextureRenderTarget2D*)UEditorAssetLibrary::LoadAsset(TEXT("/Game/VPET/NewTextureRenderTarget2D.NewTextureRenderTarget2D"));
			//if (rt)
			//{
			//	UE_LOG(LogTemp, Warning, TEXT("Source level: %s"), *rt->GetPathName());
			//	cap2D->TextureTarget = rt;
			//}
			//cap2D->TextureTarget = rt;
			// Making a RT asset
			//UFactory* ufar = &UTextureRenderTargetFactoryNew::UFactory();
			//UTextureRenderTarget2D* rt = NewObject<UTextureRenderTarget2D>();
			//UFactory* ufar = Cast<UFactory>(rt);
			//UClass* ucla = rt->GetClass();
			//FString name("TextureRT2D");
			//FString bufferDir("/Game/VPET/TextureCreation/");
			//auto myAss = IAssetTools::CreateAsset(name, bufferDir, ucla, ufar);
			//UE_LOG(LogTemp, Warning, TEXT("line 224"));
			//// Prep package
			//FString PackageName("/Game/VPET/TextureCreation/RT2D.RT2D");
			////UPackage* Package = CreatePackage(*PackageName);
			//UE_LOG(LogTemp, Warning, TEXT("line 228"));
			//UPackage* Package = CreatePackage(NULL, *PackageName);

			//UE_LOG(LogTemp, Warning, TEXT("line 231"));
			//Package->FullyLoad();

			//UE_LOG(LogTemp, Warning, TEXT("line 234"));
			////FString RTName("RenderTarget");

			////// create an unreal material asset
			////UTextureRenderTargetFactoryNew* RenderTargetFactory = NewObject<UTextureRenderTargetFactoryNew>();
			////UTextureRenderTarget2D* rt = (UTextureRenderTarget2D*)RenderTargetFactory->FactoryCreateNew(
			////	UTextureRenderTarget2D::StaticClass(), Package, *RTName, RF_Standalone | RF_Public, NULL, GWarn);
			////
			////if (rt)
			////{
			////	UE_LOG(LogTemp, Warning, TEXT("Source level: %s"), *rt->GetPathName());
			////}

			//Package->SetDirtyFlag(true);

			//UE_LOG(LogTemp, Warning, TEXT("line 249"));

			auto NewFactory = NewObject<UTextureRenderTargetFactoryNew>();
			UTextureRenderTarget2D* ScratchRenderTarget = NULL;

			UTextureRenderTarget2D* ScratchRenderTarget256;
			
			NewFactory->Width = 256;
			NewFactory->Height = 256;
			UObject* NewObj = NewFactory->FactoryCreateNew(UTextureRenderTarget2D::StaticClass(), GetTransientPackage(), NAME_None, RF_Transient, NULL, GWarn);
			ScratchRenderTarget256 = CastChecked<UTextureRenderTarget2D>(NewObj);
				
			if (ScratchRenderTarget256)
			{
				UE_LOG(LogTemp, Warning, TEXT("Source level: %s"), *ScratchRenderTarget256->GetPathName());

				ScratchRenderTarget256->RenderTargetFormat = ETextureRenderTargetFormat::RTF_RGBA8_SRGB;

				// This does NOT break :D
				cap2D->TextureTarget = ScratchRenderTarget256;

				// once
				//// capture
				//cap2D->CaptureScene();

				//// now push it
				//FString pathOut("C:/UnrealProjects/Tester427/Intermediate/VPET/matAYAYAYA.png");
				//
				//FArchive* Ar = IFileManager::Get().CreateFileWriter(*pathOut, FILEWRITE_Silent);

				//FImageUtils::ExportRenderTarget2DAsPNG(ScratchRenderTarget256, *Ar);

				//delete Ar;

				for (size_t i = 0; i < matList->Num(); i++)
				{
					FString matName = (*matList)[i]->GetName();
					
					UE_LOG(LogTemp, Warning, TEXT("Material %d: %s"), i, *matName);

					// set material
					pla2D->SetMaterial(0, (*matList)[i]);

					// capture
					cap2D->CaptureScene();

					// now push it
					FString pathOut = "C:/UnrealProjects/VPET427/Intermediate/VPET/dev1_" + matName + ".png";

					kFileList->Add(pathOut);

					//FArchive* Ar = IFileManager::Get().CreateFileWriter(*pathOut, FILEWRITE_Silent);

					//FImageUtils::ExportRenderTarget2DAsPNG(ScratchRenderTarget256, *Ar);

					//delete Ar;

					FArchive* const FileAr = IFileManager::Get().CreateFileWriter(*pathOut);// , FILEWRITE_EvenIfReadOnly);
					if (FileAr)
					{
						//ExportRenderTarget2DAsHDR
						FImageUtils::ExportRenderTarget2DAsPNG(ScratchRenderTarget256, *FileAr);
						FileAr->Close();
					}
				}
			}

			return FReply::Handled();
		}

		static FReply VisTexEncodeASTC(TArray<FString>* kFileList)
		{
			for (size_t i = 0; i < kFileList->Num(); i++)
			{
				UE_LOG(LogTemp, Warning, TEXT("File %d: %s"), i, *(*kFileList)[i]);

				FString InputFilePath = (*kFileList)[i];
				FString OutputFilePath = InputFilePath.LeftChop(3) + "astc";
				FString CompressionParameters = TEXT("6x6 -medium");

				// Compress PNG file to ASTC (using the reference astcenc.exe from ARM)
				FString Params = FString::Printf(TEXT("-cs \"%s\" \"%s\" %s"),
					*InputFilePath,
					*OutputFilePath,
					*CompressionParameters
				);

				//UE_LOG(LogTemp, Display, TEXT("Compressing to ASTC - params: %s"), *Params);
				//UE_LOG(LogTemp, Display, TEXT("Compressing to ASTC (options = '%s')..."), *CompressionParameters);

				// prepare compressor
				FString CompressorPath(FPaths::EngineDir() + TEXT("Binaries/ThirdParty/ARM/Win32/astcenc.exe"));
				//UE_LOG(LogTemp, Display, TEXT("Compressor: %s"), *CompressorPath);

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

			return FReply::Handled();
		}

		static FReply VisTexSaveTexturePNG()
		{

			// Loading from asset
			UTextureRenderTarget2D* rt = (UTextureRenderTarget2D*)UEditorAssetLibrary::LoadAsset(TEXT("/Game/VPET/TextureCreation/RT2D.RT2D"));
			if (rt)
			{
				UE_LOG(LogTemp, Warning, TEXT("RT name: %s"), *rt->GetPathName());

				FString pathOut = "C:/UnrealProjects/Tester427/Intermediate/VPET/saveRT2D.png";
				FArchive* const FileAr = IFileManager::Get().CreateFileWriter(*pathOut);// , FILEWRITE_EvenIfReadOnly);
				if (FileAr)
				{
					//ExportRenderTarget2DAsHDR
					FImageUtils::ExportRenderTarget2DAsPNG(rt, *FileAr);
					FileAr->Close();
				}
			}
			return FReply::Handled();
		}

		static FReply VisTexSaveTextureEXR()
		{

			// Loading from asset
			UTextureRenderTarget2D* rt = (UTextureRenderTarget2D*)UEditorAssetLibrary::LoadAsset(TEXT("/Game/VPET/TextureCreation/RT2D.RT2D"));
			if (rt)
			{
				UE_LOG(LogTemp, Warning, TEXT("RT name: %s"), *rt->GetPathName());

				FString pathOut = "C:/UnrealProjects/Tester427/Intermediate/VPET/saveRT2D.exr";
				FArchive* const FileAr = IFileManager::Get().CreateFileWriter(*pathOut);// , FILEWRITE_EvenIfReadOnly);
				if (FileAr)
				{
					//ExportRenderTarget2DAsHDR
					FImageUtils::ExportRenderTarget2DAsEXR(rt, *FileAr);
					FileAr->Close();
				}
			}
			return FReply::Handled();
		}

		static FReply VisTexOpenLevel(FString* kLevelName)
		{

			UE_LOG(LogTemp, Warning, TEXT("Attempt to open level: %s"), *(*kLevelName));

			UEditorLevelLibrary::LoadLevel(*kLevelName);
			
			return FReply::Handled();
		}

		static FReply VisTexCleanup()
		{
			// Delete
			FString bufferDir("/Game/VPET/TextureCreation/");

			if (UEditorAssetLibrary::DoesDirectoryExist(bufferDir))
				UEditorAssetLibrary::DeleteDirectory(bufferDir);

			return FReply::Handled();
		}

		static FReply VisTexDoItAll()
		{
			//
			// STEP 1
			// 
			// Pick all actors that have the edit select tab
			// Add a list of materials
			TArray<UMaterialInterface*> matList;
			// Print the material names
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
			// Fresh level
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
					FString pathOut = "C:/UnrealProjects/VPET427/Intermediate/VPET/dev1_" + matName + ".png";

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
				FString CompressorPath(FPaths::EngineDir() + TEXT("Binaries/ThirdParty/ARM/Win32/astcenc.exe"));

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
	// UI
	//
	return SNew(SDockTab)
		.TabRole(ETabRole::NomadTab)
		[
			SNew(SVerticalBox)
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
		.Text(LOCTEXT("WidgetTexDebugText", "Debug texture"))
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("VisTexGrabMatListLabel", "Grab Material List"))
		.OnClicked_Static(&Functions::VisTexGrabMatList, &matList)
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("VisTexLoadLevelLabel", "New Level"))
		.OnClicked_Static(&Functions::VisTexNewLevel, &sourceLevelName)
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("VisTexAddContentLabel", "Add content"))
		.OnClicked_Static(&Functions::VisTexAddContent, &matList, &fileList)
		]
	//+ SHorizontalBox::Slot()
	//	.AutoWidth()
	//	[
	//		SNew(SButton)
	//		.Text(LOCTEXT("VisTexSsaveTexPNGLabel", "Save texture PNG"))
	//	.OnClicked_Static(&Functions::VisTexSaveTexturePNG)
	//	]
	//+ SHorizontalBox::Slot()
	//	.AutoWidth()
	//	[
	//		SNew(SButton)
	//		.Text(LOCTEXT("VisTexSsaveTexEXRLabel", "Save texture EXR"))
	//	.OnClicked_Static(&Functions::VisTexSaveTextureEXR)
	//	]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("VisTexEncodeASTCLabel", "Encode to ASTC"))
		.OnClicked_Static(&Functions::VisTexEncodeASTC, &fileList)
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("VisTexOpenLevelLabel", "Reopen Level"))
		.OnClicked_Static(&Functions::VisTexOpenLevel, &sourceLevelName)
		]
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("VisTexCleanupLabel", "Clean-up"))
		.OnClicked_Static(&Functions::VisTexCleanup)
		]
		]
	+ SVerticalBox::Slot()
		.HAlign(HAlign_Center)
		.AutoHeight()
		.Padding(4)
		[SNew(SHorizontalBox)
	+ SHorizontalBox::Slot()
		.AutoWidth()
		[
			SNew(SButton)
			.Text(LOCTEXT("VisTexDoItAllLabel", "Do it all!"))
		.OnClicked_Static(&Functions::VisTexDoItAll)
		]
		]
		];
}

void FVPETTexturerModule::PluginButtonClicked()
{
	FGlobalTabmanager::Get()->TryInvokeTab(VPETTexturerTabName);
}

void FVPETTexturerModule::RegisterMenus()
{
	// Owner will be used for cleanup in call to UToolMenus::UnregisterOwner
	FToolMenuOwnerScoped OwnerScoped(this);

	{
		UToolMenu* Menu = UToolMenus::Get()->ExtendMenu("LevelEditor.MainMenu.Window");
		{
			FToolMenuSection& Section = Menu->FindOrAddSection("WindowLayout");
			Section.AddMenuEntryWithCommandList(FVPETTexturerCommands::Get().OpenPluginWindow, PluginCommands);
		}
	}

	{
		UToolMenu* ToolbarMenu = UToolMenus::Get()->ExtendMenu("LevelEditor.LevelEditorToolBar");
		{
			FToolMenuSection& Section = ToolbarMenu->FindOrAddSection("Settings");
			{
				FToolMenuEntry& Entry = Section.AddEntry(FToolMenuEntry::InitToolBarButton(FVPETTexturerCommands::Get().OpenPluginWindow));
				Entry.SetCommandList(PluginCommands);
			}
		}
	}
}

#undef LOCTEXT_NAMESPACE
	
IMPLEMENT_MODULE(FVPETTexturerModule, VPETTexturer)