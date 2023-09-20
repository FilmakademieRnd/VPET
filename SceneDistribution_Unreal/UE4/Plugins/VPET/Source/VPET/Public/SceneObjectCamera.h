// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#pragma once

#define DEG2RAD (3.14159265/180.0)

#include "CoreMinimal.h"
#include "SceneObject.h"

#include "Camera/CameraActor.h"
#include "Camera/CameraComponent.h"
#include "CineCameraActor.h"
#include "CineCameraComponent.h"

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

	ACameraActor* kCam = NULL;
	UCameraComponent* kCamComp = NULL;
	ACineCameraActor* kCineCam = NULL;
	UCineCameraComponent* kCineCamComp = NULL;

	// Parameter buffers
	Parameter<float>* FOV_Vpet_Param;
	Parameter<float>* Aspect_Vpet_Param;
	Parameter<float>* Near_Vpet_Param;
	Parameter<float>* Far_Vpet_Param;
	Parameter<float>* FocDist_Vpet_Param;
	Parameter<float>* Aperture_Vpet_Param;
	Parameter<FVector2D>* Sensor_Vpet_Param;
};
