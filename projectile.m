classdef projectile
    properties

        % Models the 7.62x51mm M62 ball
        m = 0.0097198359; % Mass [kg]
        g = 9.8065; % Gravity constant 
        cd_norm = 0.280; % Drag coefficient [-]
        cd_Cur = 0; %
        Temp_Cur = 0 % 
        dViscosity = 0; % Dynamic Viscosity [kg/m.s]
        F_sA = 4.80683e-5 ; % Surface Area [m2]
        Re = 6356766; % Constant mass of earth [m]
        Ma =  28.9644; % Molar Mass of air [g/Mol]
        Rcost = 8.31432; % Gas constant [J/Mol.K]
        temp_Inc = 6.5; % Temperature increment per km [k/km]
        beta = 1.458e-6; % Constant [s.m/k]
        s = 110.4; % Sutherland's Constant [k]
        mach = 0; % Mach Number
        R = 8.31446; % Gas Constant 
        mol = 0.0289644; % Molar mass of air average [kg/mol]
        f_drag = 0; % Drag force in lateral direction
        energy = 0; 
        rotation = 0;


    end

    methods
        function cd = current_drag_coefficient(speed, Temp_Cur)
            
            % Calculate Mach Number
            c = sqrt(  (1.4* 8.31446) / 0.0289644  ) * sqrt(Temp_Cur);
            this.mach = speed/c;

            % Given Mach Number Calculate cd Based on G7 Table
            g7DragTable = readmatrix("drag coefficient.xlsx",'Sheet','Data','Range','B2:C85');

            for i = 1:size(g7DragTable,1)
                if g7DragTable(i,1) == ceil(this.mach*4)/4
                    this.cd_Cur = g7DragTable(i,2);
                end
            end

            cd.current = this.cd_Cur;
        end

        function drag = dragforce(speed, cd_Cur, Temp_Cur)

            % Calculate Air Density 
            this.rho_Cur = ( (Temp_Cur / 288.15 )^((this.g*this.mol)/(this.R  * 6.5)) * this.mol * 101325 ) / (1000 * 6.5 * Temp_Cur);
            this.f_drag = (1/2) * this.rho_Cur * cd_Cur * speed^2 * F_sA;
            drag.current= 0;

        end










    end
end




