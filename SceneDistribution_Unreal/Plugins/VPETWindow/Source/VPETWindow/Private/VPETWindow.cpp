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

#include "VPETWindow.h"
#include "VPETWindowStyle.h"
#include "VPETWindowCommands.h"
#include "LevelEditor.h"
#include "Widgets/Docking/SDockTab.h"
#include "Widgets/Layout/SBox.h"
#include "Widgets/Text/STextBlock.h"
#include "Framework/MultiBox/MultiBoxBuilder.h"

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
		FVPETWindowCommands::Get().OpenPluginWindow,
		FExecuteAction::CreateRaw(this, &FVPETWindowModule::PluginButtonClicked),
		FCanExecuteAction());

	FLevelEditorModule& LevelEditorModule = FModuleManager::LoadModuleChecked<FLevelEditorModule>("LevelEditor");

	{
		TSharedPtr<FExtender> MenuExtender = MakeShareable(new FExtender());
		MenuExtender->AddMenuExtension("WindowLayout", EExtensionHook::After, PluginCommands, FMenuExtensionDelegate::CreateRaw(this, &FVPETWindowModule::AddMenuExtension));

		LevelEditorModule.GetMenuExtensibilityManager()->AddExtender(MenuExtender);
	}

	{
		TSharedPtr<FExtender> ToolbarExtender = MakeShareable(new FExtender);
		ToolbarExtender->AddToolBarExtension("Settings", EExtensionHook::After, PluginCommands, FToolBarExtensionDelegate::CreateRaw(this, &FVPETWindowModule::AddToolbarExtension));

		LevelEditorModule.GetToolBarExtensibilityManager()->AddExtender(ToolbarExtender);
	}

	FGlobalTabmanager::Get()->RegisterNomadTabSpawner(VPETWindowTabName, FOnSpawnTab::CreateRaw(this, &FVPETWindowModule::OnSpawnPluginTab))
		.SetDisplayName(LOCTEXT("FVPETWindowTabTitle", "VPETWindow"))
		.SetMenuType(ETabSpawnerMenuType::Hidden);
}

void FVPETWindowModule::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.
	FVPETWindowStyle::Shutdown();

	FVPETWindowCommands::Unregister();

	FGlobalTabmanager::Get()->UnregisterNomadTabSpawner(VPETWindowTabName);
}

TSharedRef<SDockTab> FVPETWindowModule::OnSpawnPluginTab(const FSpawnTabArgs& SpawnTabArgs)
{
	struct Functions
	{

		static void AddSendChain(AActor* aActor)
		{
			// Register
			aActor->Modify();
			// Add
			if (aActor->Tags.Find("Send") == INDEX_NONE)
				aActor->Tags.Add(FName("Send"));
			// Wal up attachment chain
			AActor* aParActor = aActor->GetAttachParentActor();
			if (aParActor)
				AddSendChain(aParActor);
		}

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

					// Hide
					lActor->SetActorHiddenInGame(false);
				}
			}
			GEditor->EndTransaction();
			return FReply::Handled();
		}

	};


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
	FGlobalTabmanager::Get()->InvokeTab(VPETWindowTabName);
}

void FVPETWindowModule::AddMenuExtension(FMenuBuilder& Builder)
{
	Builder.AddMenuEntry(FVPETWindowCommands::Get().OpenPluginWindow);
}

void FVPETWindowModule::AddToolbarExtension(FToolBarBuilder& Builder)
{
	Builder.AddToolBarButton(FVPETWindowCommands::Get().OpenPluginWindow);
}

#undef LOCTEXT_NAMESPACE

IMPLEMENT_MODULE(FVPETWindowModule, VPETWindow)