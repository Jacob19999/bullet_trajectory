# Unity Physically Based Bullet
C# Script for the use in game engines that simulated a 3D virtual bullet's physical trajectory. Bullet class script contains the following features (Allows the user to initialize any bullet of any caliber.) 

<Features>
Implementation of drag model used by the US Army testing data (G1, G2, G5, G6, G7, G8, GS). 
Most aspects of bullet trajectory physics: ( Gravity , Drag , Spin Drift , Coriolis , Centripetal , Wind ). The calculation of these elements are non linear which means a function cannot be used to estimate it's path. Each elements as mentioned above has to be calculated in discrete steps that is synced to the frame rate of the game, and its acclearation effects integrated into velocity which is finally integrated into displacement. 
  
Addational Initial conditions considered: (Firing angle , temperature , pressure , rifle twist , firing azimuth north south referenced , and latitude ). 

As of current, the script would be able to simulate a bullet trajectory to +- 5% MOA at 1000 yards compared to published data.

Addational area of implementation (WIP):
1. Yawing moment damping factor which affects bullets in close ranges.
2. Energy conservation (For bullet penetration)
3. Incidence angles (Richiots ricochet)
4. Bullet shatter / fragmentation
5. Dynamic load balance to optimize performance when there are many bullets.
6. Collision detection. 


Sample trajectory of 7.62 NATO M82 Ball in Matlab


![image](https://user-images.githubusercontent.com/26366586/233151972-0e59c601-ea14-4239-96a9-c3218960f0f2.png)

![image](https://user-images.githubusercontent.com/26366586/233152119-4353c99c-d182-4b3c-a43f-acdcfda424e6.png)


