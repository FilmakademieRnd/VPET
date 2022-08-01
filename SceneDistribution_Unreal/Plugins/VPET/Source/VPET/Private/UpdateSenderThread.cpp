// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "UpdateSenderThread.h"

// Update sender thread
void UpdateSenderThread::DoWork()
{
	DOL(doLog, Warning, "[VPET2 SEND Thread] zeroMQ update sender thread running");

	// Variables for socket check test
	int type;
	size_t type_size = sizeof(type);

	while (1)
	{
		// Try something using the socket just to be able to stop the thread when the socket is closed by EndPlay
		try {
			socket->getsockopt(ZMQ_TYPE, &type, &type_size);
		}
		catch (const zmq::error_t& e)
		{
			FString errName = FString(zmq_strerror(e.num()));
			DOL(doLog, Error, "[SEND Thread] socket exception: %s", *errName);
			return;
		}

		// Process messages
		int count = 0;
		for (size_t i = 0; i < msgData->size(); i++)
		{
			count++;

			// Send message
			DOL(doLog, Log, "[SEND Thread] Send message length: %d", msgLen->at(i));
			zmq::message_t responseMessage((void*)msgData->at(i), msgLen->at(i), NULL);
			try {
				socket->send(responseMessage);
			}
			catch (const zmq::error_t& e)
			{
				FString errName = FString(zmq_strerror(e.num()));
				DOL(doLog, Error, "[SEND Thread] send exception: %s", *errName);
				return;
			}
		}

		// Clean processed messages
		msgData->erase(msgData->begin(), msgData->begin() + count);
		msgLen->erase(msgLen->begin(), msgLen->begin() + count);

		// Safety halt
		// Keeps the thread from running wild
		Sleep(10);
	}
}