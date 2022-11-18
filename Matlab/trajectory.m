clear all;
close all;


traj1 = readtable('Simulation Data.xlsx','Sheet','Sheet2','Range','A1:D167');
traj2 = readtable('Simulation Data.xlsx','Sheet','Sheet2','Range','F1:I276');
traj3 = readtable('Simulation Data.xlsx','Sheet','Sheet2','Range','K1:N336');
traj4 = readtable('Simulation Data.xlsx','Sheet','Sheet2','Range','P1:S601');

plot3(traj1.X,traj1.Z,traj1.Y,'-');
hold on;
plot3(traj2.X,traj2.Z,traj2.Y,'-');
hold on;
plot3(traj3.X,traj3.Z,traj3.Y,'-');
hold on;
plot3(traj4.X,traj4.Z,traj4.Y,'-');
hold on;




grid on
axis square
xlabel('X')
ylabel('Z')
zlabel('Y')
axis equal;
camproj('perspective')