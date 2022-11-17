clear

% Parameters
rho_sl = 1.225; % Air density [kg/m3]
sa = 0.00000635614; % Surface Area [m2]
cd = 0.280; % Drag coefficient [-]
m = 0.062; % Mass [kg]
g = 9.8065; % Gravitational acceleration [m/s2]
me = 5.9722e24; % Constant mass of earth [kg]
re = 6378000 ; % Constant mass of earth [m]
time_final = 30; % Simulation final time [s]
time_step = 10 / 1000; % Integration Time Step [ms]
g = 9.8065; % Gravitational acceleration [m/s2]
me = 5.9722e24; % Constant mass of earth [kg]
re = 6378000 ; % Constant mass of earth [m]
c = 0; % Speed of sound [m/s]
R = 8.31446; % Gas Constant 
mol = 0.0289644; % Molar mass of air average [kg/mol]
k = 0; % Temperature [Kelvin] 

%Initial conditions
V0=993; %Initial speed [m/s]
% X,Y,Z position from initial
pos = [0 0 0];

alpha_init = 0;

% G7 lookuptable calculator
mach = 0.1435;
g7DragTable = readmatrix("drag coefficient.xlsx",'Sheet','Data','Range','B2:C85');
for i = 1:size(g7DragTable,1)
    if g7DragTable(i,1) == ceil(mach*4)/4;
        cd = g7DragTable(i,2);
    end
end

% Speed of Sound 

k = 30 + 273.15;

c = sqrt(  (1.4* 8.31446) / 0.0289644  ) * sqrt(k);



% Iterate time step
for t = 0 :time_step: time_final


end
