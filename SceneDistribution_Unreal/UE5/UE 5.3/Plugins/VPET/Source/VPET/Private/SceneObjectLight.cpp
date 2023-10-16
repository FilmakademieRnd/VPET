// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneObjectLight.h"

// Message parsing pre-declarations
void UpdateColor(std::vector<uint8_t> kMsg, AActor* actor);
void UpdateIntensity(std::vector<uint8_t> kMsg, AActor* actor);

// Called when the game starts
void USceneObjectLight::BeginPlay()
{
	Super::BeginPlay();

	kLit = Cast<ALight>(thisActor);
	if (kLit)
	{
		FVector4d col = (FVector4d)kLit->GetLightColor();
		float lit = kLit->GetBrightness() * lightFactor;

		Col_Vpet_Param = new Parameter<FVector4d>(col, thisActor, "color", &UpdateColor, this);
		lit_Vpet_Param = new Parameter<float>(lit, thisActor, "intensity", &UpdateIntensity, this);
	}
}

// Using the update loop to check for local parameter changes
void USceneObjectLight::TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction)
{
	Super::TickComponent(DeltaTime, TickType, ThisTickFunction);

	if (_lock)
		return;
	
	if (kLit)
	{
		FVector4d col = kLit->GetLightColor();
		float lit = kLit->GetBrightness() * lightFactor;

		if (col != Col_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("COL CHANGE"));
			ParameterObject_HasChanged.Broadcast(Col_Vpet_Param);
			Col_Vpet_Param->setValue(col);
		}
		if (lit != lit_Vpet_Param->getValue())
		{
			UE_LOG(LogTemp, Warning, TEXT("LIT CHANGE"));
			ParameterObject_HasChanged.Broadcast(lit_Vpet_Param);
			lit_Vpet_Param->setValue(lit);
		}
	}
}

	/*float msgVal = colBuffer[0];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = colBuffer[1];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = colBuffer[2];
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	msgVal = 1; // alpha
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);*/

	/*float msgVal = litBuffer / lightFactor;
	memcpy(responseMessageContent, (char*)&msgVal, sizeof(float));
	responseMessageContent += sizeof(float);
	*/

// Parses a message for color change
void UpdateColor(std::vector<uint8_t> kMsg, AActor* actor)
{
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Try Type COLOR"));
	ALight* kLit = Cast<ALight>(actor);
	if (kLit)
	{
		float lR = *reinterpret_cast<float*>(&kMsg[0]);
		float lG = *reinterpret_cast<float*>(&kMsg[4]);
		float lB = *reinterpret_cast<float*>(&kMsg[8]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type COLOR: %f %f %f"), lR, lG, lB);

		kLit->SetLightColor(FLinearColor(lR, lG, lB));
	}
}

// Parses a message for intensity change
void UpdateIntensity(std::vector<uint8_t> kMsg, AActor* actor)
{
	UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Try Type INTENSITY"));
	ALight* kLit = Cast<ALight>(actor);
	if (kLit)
	{
		float lK = *reinterpret_cast<float*>(&kMsg[0]);
		UE_LOG(LogTemp, Warning, TEXT("[SYNC Parse] Type INTENSITY: %f"), lK);

		float lightFactor = 0.2;
		kLit->SetBrightness(lK / lightFactor);
	}
}