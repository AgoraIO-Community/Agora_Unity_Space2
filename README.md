# Agora Space 2
This demo combines the features of Agora RTC SDK and Signaling SDK to enable multiplayer interaction within a 3D Unity space.  The project is an rewrite of the previous [Agora_Unity_Space](https://github.com/AgoraIO-Community/Agora_Spaces_Unity).  The main difference is that previous version leverages Mirror SDK as synchronize remote objects.  Also, it requires a saparate server or a player serves as both client and server node. In this implementation, the Agora Signaling SDK takes care of the transform synchronization and we don't require a separate server node anymore.

## Sample Scene

https://github.com/icywind/Agora_Unity_Space2/assets/1261195/c054abc8-6289-4f83-aa73-8861511098c1


## Developer Environment Prerequisites
- Unity3d 2021.3 LTS
- Agora Developer Account
- Agora Signaling SDK ([version 2.1.9](https://download.agora.io/sdk/release/Agora_Unity_RTM_SDK_v2.1.9.zip) or newer)
	
## Quick Start

This section shows you how to prepare, build, and run the sample application.


### Obtain an App ID


To build and run the sample application, get an App ID:

1. Create a developer account at [agora.io](https://dashboard.agora.io/signin/). Once you finish the signup process, you will be redirected to the Dashboard.

2. Navigate in Agora Console on the left to **Projects** > **More** > **Create** > **Create New Project**.

3. Save the **App ID** from the Dashboard for later use.

  

### Run the Application

  

#### Build for desktop

1. Clone this repo and open the project from this folder
2. [Download](https://docs.agora.io/en/sdks?platform=unity) the latest RTC SDK and Signaling SDK (aka RTM SDK)
3. Fill in App ID and Token (if enabled)  ![Game_-_AppID](https://github.com/icywind/Agora_Unity_Space2/assets/1261195/9986ad20-bd03-4f25-8949-590df19f7fca)

4. Make sure if your AppID has token or not.  Things won't work if you don't supply a token if your AppID requires one.  We recommend use an AppID for testing first before applying token logic.
5. Make a build and run it with the Editor


## License

The MIT License (MIT).


