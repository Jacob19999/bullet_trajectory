# bullet_trajectory

C# Script for the use in game engines that simulated a 3D virtual bullet's physical trajectory. Bullet class script contains the following features (Allows the user to initialize any bullet of any caliber . 

<Features>
This is due to the implementation of drag model (G1, G2, G5, G6, G7, G8, GS) used by the US Army testing data) . 

The scrpt implements almost all aspects of bullet physics ( Gravity , Drag , Spin Drift , Coriolis , Centripetal , Wind ) and taking into account of (Firing angle , temperature , pressure , rifle twist , firing azimuth north south referenced , and latitude ). 

As of current, the script would be able to simulate a bullet trajectory to +- 5% MOA at 1000 yards compared to published data.

Addational area of implementation (WIP):
1. Yawing moment damping factor which affects bullets in close ranges.
2. Energy conservation (For bullet penetration)
3. Incidence angles (Richiots ricochet)
4. Bullet shatter / fragmentation


Sample trajectory of 7.62 NATO M82 Ball in Matlab


![image](https://user-images.githubusercontent.com/26366586/233151972-0e59c601-ea14-4239-96a9-c3218960f0f2.png)

![image](https://user-images.githubusercontent.com/26366586/233152119-4353c99c-d182-4b3c-a43f-acdcfda424e6.png)


https://www.jacob-space.com/
