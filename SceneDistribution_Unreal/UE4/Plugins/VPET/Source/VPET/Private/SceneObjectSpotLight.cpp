// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObjectSpotLight.h"

// Message parsing pre-declarations
void UpdateRange(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateAngle(std::vector<uint8_t> kMsg, AActor* actor);

// Called when the game starts
void USceneObjectSpotLight::BeginPlay()
{
	Super::BeginPlay();

	kSpotLgt = Cast<ASpotLight>(kLit);
	if (kSpotLgt)
		spotLgtCmp = kSpotLgt->SpotLightComponent;
	if (spotLgtCmp)
	{
		float range = spotLgtCmp->AttenuationRadius * rangeFactor;
		float angle = spotLgtCmp->OuterConeAngle * angleFactor;

		Range_Vpet_Param = new Parameter<float>(range, thisActor, "range", &UpdateRange, this);
		Angle_Vpet_Param = new Parameter<float>(angle, thisActor, "spot angle", &UpdateAngle, this);
	}
}

// Using the update loop to check for local parameter changes
void USceneObjectSpotLight::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (_lock)
		return;
	
	if (spotLgtCmp)
	{
		float range = spotLgtCmp->AttenuationRadius * rangeFactor;
		float angle = spotLgtCmp->OuterConeAngle * angleFactor;

		if (range != Range_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("RANGE CHANGE"));
			ParameterObject_HasChanged.Broadcast(Range_Vpet_Param);
			Range_Vpet_Param->setValue(range);
		}
		if (angle != Angle_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("ANGLE CHANGE"));
			ParameterObject_HasChanged.Broadcast(Angle_Vpet_Param);
			Angle_Vpet_Param->setValue(angle);
		}
	}
}

// Parses a message for range change
void UpdateRange(std::vector<uint8_t> kMsg, AActor* actor)
{
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Try Type RANGE"));
	ASpotLight* kSpotLgt = Cast<ASpotLight>(actor);
	USpotLightComponent* spotLgtCmp = NULL;
	if(kSpotLgt)
		spotLgtCmp = kSpotLgt->SpotLightComponent;
	if (spotLgtCmp)
	{
		float rangeFactor = 0.005;

		float lK = *reinterpret_cast<float*>(&kMsg[0]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type RANGE: %f"), lK);

		spotLgtCmp->AttenuationRadius = lK / rangeFactor;
	}
}

// Parses a message for angle change
void UpdateAngle(std::vector<uint8_t> kMsg, AActor* actor)
{
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Try Type SPOT ANGLE"));
	ASpotLight* kSpotLgt = Cast<ASpotLight>(actor);
	USpotLightComponent* spotLgtCmp = NULL;
	if(kSpotLgt)
		spotLgtCmp = kSpotLgt->SpotLightComponent;
	if (spotLgtCmp)
	{
		float angleFactor = 2.0;

		float lK = *reinterpret_cast<float*>(&kMsg[0]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type SPOT ANGLE: %f"), lK);

		spotLgtCmp->OuterConeAngle = lK / angleFactor;
	}
}
