// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#define DEG2RAD (3.14159265/180.0)

#include "CoreMinimal.h"
#include "SceneObject.h"

#include "Camera/CameraActor.h"
#include "Camera/CameraComponent.h"

#include "SceneObjectCamera.generated.h"

/**
 * 
 */
UCLASS()
class VPET_API USceneObjectCamera : public USceneObject
{
	GENERATED_BODY()
	
protected:
	// Called when the game starts
	virtual void BeginPlay() override;

	// Called every frame
	virtual void TickComponent(float DeltaTime, ELevelTick TickType, FActorComponentTickFunction* ThisTickFunction) override;

	ACameraActor* kCam;
	UCameraComponent* kCamComp;

	// Parameter buffers
	float fovBuffer;
	float aspectBuffer;
	float nearBuffer;
	float farBuffer;
	float focDistBuffer;
	float apertureBuffer;
	FVector2D sensorBuffer;

	void EncodeParameterFovMessage();
	void EncodeParameterAspectMessage();
	void EncodeParameterNearMessage();
	void EncodeParameterFarMessage();
	void EncodeParameterFocDistMessage();
	void EncodeParameterApertureMessage();
	void EncodeParameterSensorMessage();
};
