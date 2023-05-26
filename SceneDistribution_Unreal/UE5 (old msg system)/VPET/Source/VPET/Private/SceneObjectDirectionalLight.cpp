// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObjectDirectionalLight.h"

// Called when the game starts
void USceneObjectDirectionalLight::BeginPlay()
{
	Super::BeginPlay();
}

// Using the update loop to check for local parameter changes
void USceneObjectDirectionalLight::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);
}
