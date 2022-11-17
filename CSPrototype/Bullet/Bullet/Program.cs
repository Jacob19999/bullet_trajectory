// Bullet Trajectory calculator in 3D vector Written by Jacob Thang for prototyping in Unity. 


using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Program
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // Drag Model , Mass (Grains), Twist (inch/Turn), Diameter (M), Length (M), Pressure (pa) , Temp(k)

            var bullet1 = new Bullet(Bullet.dragModel.G7, 150, 822, 10, 0.0078232, 0.0338328, 101325, 288.16);
            bullet1.fire(0);
            Console.WriteLine("Stability Factor : " + bullet1.GetStabilityFactor());
            Console.WriteLine("Drag Coefficient : " + bullet1.getDragCoefficient());
            Console.WriteLine("Retardation : " + bullet1.getRetardation());


            //bullet1.update();


            
            for (int i = 0; i < 400; i++)
            {

                bullet1.update();
                Thread.Sleep(50);

                if (bullet1.projectileImpact == true)
                {
                    Console.WriteLine("Porjectile Imapcted Ground.");
                    break;
                }


            }
            

            /////////////////////////////////// Test Code ///////////////////////////////
            double A = 0;
            double N = 0;

            A = 2.33355948302505e-03f; N = 1.52693913274526f;

            double velFps = 2696.85;
            double m_retardation = 0;

            if (A != -1 && N != -1 && velFps > 0 && velFps < 100000)
            {
                m_retardation = A * Math.Pow(velFps, N) / 0.203;
                m_retardation = m_retardation / 3.2808399;
            }

            //Console.WriteLine("Retardation Test : " + m_retardation);
            /////////////////////////////////// Test Code ///////////////////////////////


        }
    }
}


class Bullet
{

    //G1 – G1 projectiles are flatbase bullets with 2 caliber nose ogive and are the most common type of bullet.
    //G2 – bullets in the G2 range are Aberdeen J projectiles
    //G5 – G5 bullets are short 7.5 degree boat-tails, with 6.19 caliber long tangent ogive
    //G6 – G6 are flatbase bullets with a 6 cailber secant ogive
    //G7 – Bullets with the G7 BC are long 7.5 degree boat-tails, with 10 caliber tangent ogive, and are very popular with manufacturers for extremely low-drag bullets.

    // Vectors
    double[] start_Pos = new double[3];
    double[] pos = new double[3];
    double[] prev_pos = new double[3];
    double[] spin = new double[3];
    double[] lateralAccel = new double[3];
    double[] drag_Vector = new double[3];
    double[] velocity_Vector = new double[3];
    double[] centripetal_Vector = new double[3];
    double[] coriolis_Vector = new double[3];
    double[] prev_coriolis = new double[3];
    double[] wind_Vector = new double[3];
    double[] gravity_Vector = new double[3];
    double[] velocity_Vector_dt = new double[3];

    // Metric
    private double Re = 6356766;
    private double Me = 5.9722e24;
    private double g = -9.80665;
    private double k_omega = 0.000072921159;
    private double mass;
    private double mass_ibs;
    private double twist;
    private double muzzle_Velocity;
    private double bullet_Dia;
    private double bullet_Len;
    private double baro;
    private double pressure;
    private double temp_k;
    private double front_Area;
    private double dt;
    private double bullet_Direction;
    private double elapsed_Ms;


    //Imperial 
    private double muzzle_Vel_Fps;
    private double grains;
    private double calibers;
    private double bullet_Dia_Inch;
    private double bullet_Len_Inch;
    private double temp_F;
    private double twist_Calibers;
    private double ballistic_Coefficient;
    private double sectional_Density;
    private double stability_Fac;

    //
    public bool projectileImpact { get; set; } 

    public enum dragModel { G1, G2, G3, G4, G5, G6, G7, G8, GS };
    public dragModel drag_Model;


    public Bullet(dragModel dModel , double grains, double muzzleVelocity, double barrelTwist, double bulletDia, double bulletLen, double pressure, double temp)
    {

        this.drag_Model = dModel;
        this.mass = grains * 0.0000647989;
        this.mass_ibs = mass * 2.20462;
        this.twist = barrelTwist;
        this.bullet_Dia = bulletDia;
        this.bullet_Len = bulletLen;
        this.temp_k = temp;
        this.muzzle_Velocity = muzzleVelocity;
        this.pressure = pressure;
        this.grains = this.mass * 15432.4;
        this.bullet_Dia_Inch = this.bullet_Dia * 39.3701;
        this.bullet_Len_Inch = this.bullet_Len * 39.3701;
        this.calibers = this.bullet_Len / this.bullet_Dia;
        this.twist_Calibers = barrelTwist / this.bullet_Dia_Inch;
        this.front_Area = Math.PI * Math.Pow(this.bullet_Dia / 2, 2);
        this.sectional_Density = this.mass_ibs / Math.Pow(this.bullet_Dia_Inch, 2);
        this.dt = 0.05; // Fixed Delta Time (Seconds)
        this.elapsed_Ms = 0;

        stability_Fac = GetStabilityFactor();

    }




    public void fire(double firing_angle)
    {

        this.pos[0] = 0;
        this.pos[1] = 500;
        this.pos[2] = 0;

        firing_angle = firing_angle * (Math.PI / 180);

        //Y
        this.velocity_Vector[1] = Math.Sin(firing_angle) * this.muzzle_Velocity ;
        //X
        this.velocity_Vector[0] = Math.Cos(firing_angle) * this.muzzle_Velocity ;
        //Z
        this.velocity_Vector[2] = 0;

        velocity_Vector_dt[0] = this.velocity_Vector[0] * dt;
        velocity_Vector_dt[1] = this.velocity_Vector[1] * dt;
        velocity_Vector_dt[2] = 0;

        projectileImpact = false;

        //Console.WriteLine("Velocity x = " + velocity_Vector[0] + " y = " + velocity_Vector[1] + " z = " + velocity_Vector[2]);

    }


    public void update()
    {

        updateWind();
        getGravity();
        getDrag();

        updatePosition();
        
        if (pos[1] < 0 )
        {
            projectileImpact = true;
        }

        print();

    }

    public void print()
    {


        elapsed_Ms = elapsed_Ms + (this.dt) * 1000;


        Console.WriteLine(Math.Round(elapsed_Ms/1000, 1) + "s " + Math.Round(getRelativeSpeed(), 1) + "m/s "  + Math.Round(getMach(getRelativeSpeed(), temp_k),2) + " mach , X = " + Math.Round(this.pos[0],4) + "m , Y = " + Math.Round(this.pos[1], 4) + "m , Z = " + Math.Round(this.pos[2], 4)+"m");

    }


    public void updatePosition()
    {
        this.prev_pos = this.pos;

        // Integrate Gravity
        this.pos = vectorOperation(this.pos, this.gravity_Vector, "+");
        this.pos[0] = pos[0] + gravity_Vector[0] * dt;
        this.pos[1] = pos[1] + gravity_Vector[1] * dt;
        this.pos[2] = pos[2] + gravity_Vector[2] * dt;

        // Integrate Drag
        this.velocity_Vector = vectorOperation(this.velocity_Vector, this.drag_Vector, "-");
        this.pos[0] = pos[0] + velocity_Vector[0] * dt;
        this.pos[1] = pos[1] + velocity_Vector[1] * dt;
        this.pos[2] = pos[2] + velocity_Vector[2] * dt;

        //Console.WriteLine("Velocity x = " + velocity_Vector[0] + " y = " + velocity_Vector[1] + " z = " + velocity_Vector[2]);


    }


    public void updateWind()
    {
        // air speed m/s
        this.wind_Vector[0] = 2;
        this.wind_Vector[1] = 1;
        this.wind_Vector[2] = 5;

        this.wind_Vector[0] = this.wind_Vector[0] * dt;
        this.wind_Vector[1] = this.wind_Vector[1] * dt;
        this.wind_Vector[2] = this.wind_Vector[2] * dt;

    }

    public void getGravity()
    {

        // Reference from: 2.2 Ballistic Model: Empirical Data to Determine Transonic Drag Coefficient pdf

        double r = Math.Sqrt(Math.Pow(pos[0], 2) + Math.Pow( ((pos[1]) + this.Re), 2) );

        this.gravity_Vector[0] = 0;
        this.gravity_Vector[1] = this.g * dt;
        this.gravity_Vector[2] = 0;

        //Console.WriteLine("Gravity = " + gravity_Vector[0] + " , " + gravity_Vector[1] + " , " + gravity_Vector[2]);

    }

    public void getDrag()
    {

        double _drag = this.dt * getRetardation();

        double[] v_drag = { 0, 0, 0 };

        v_drag = vectorNormalize(getTrueSpeed());

        this.drag_Vector[0] = v_drag[0] * _drag;
        this.drag_Vector[1] = v_drag[1] * _drag;
        this.drag_Vector[2] = v_drag[2] * _drag;


        //Console.WriteLine("Drag = " + drag_Vector[0] + " , " + drag_Vector[1] + " , " + drag_Vector[2]);

        //Console.WriteLine(" Drag [0] = " + this.drag[0]);
        //Console.WriteLine(" Drag [1] = " + this.drag[1]);
        //Console.WriteLine(" Drag [2] = " + this.drag[2]);

    }


    private double[] getTrueSpeed()
    {

        // Get Velocity Vector3 from unity. Using rb.Velocity.Magnitude
        this.velocity_Vector = vectorOperation(this.velocity_Vector, this.wind_Vector, "+");

        return velocity_Vector;
    }

    private double getRelativeSpeed()
    {

        // Get Velocity Vector3 from unity. Using rb.Velocity.Magnitude
        this.velocity_Vector = vectorOperation(this.velocity_Vector, this.wind_Vector, "-");
        double relative_Velocity = vectorlength(this.velocity_Vector);

        return relative_Velocity;
    }

    private double getMach(double relativeSpeed, double temp_k)
    {
        double _mach;
        double _c;

        // Speed of sound (m/s)
        // C0 = 20.046 m/s @ 1k

        _c = 20.046 * Math.Sqrt(temp_k);
        _mach = relativeSpeed / _c;

        return _mach;
    }

    public double getRetardation()
    {
        // Bullet Drag Force based on cd and kd
        // Reference https://www.jbmballistics.com/ballistics/topics/cdkd.shtml

        double _cd = getDragCoefficient();
        double _kd = _cd * 0.3927;
        double air_Density = (this.pressure / ((287.05) * this.temp_k));
        double rel_Vel = getRelativeSpeed();
        double drag = 0;

        this.ballistic_Coefficient = this.mass_ibs / (_cd * ( Math.Pow( this.bullet_Dia_Inch , 2) * Math.PI ));
        drag = (0.5 * air_Density * Math.Pow(rel_Vel, 2) * this.front_Area * _cd);

        //Console.WriteLine("Ballistic Coefficient = " + this.ballistic_Coefficient);
        //Console.WriteLine("Sectional Density = " + this.sectional_Density);
        //Console.WriteLine("Air Density = " + air_Density);
        //Console.WriteLine("Velocity = " + rel_Vel);
        //Console.WriteLine("Mach = " + rel_Vel);
        //Console.WriteLine("Area = " + this.front_Area);
        //Console.WriteLine("cd = " + _cd);

        return drag / this.mass; 
    }

    public double getDragCoefficient()
    {
        // Drag Model data imported from https://www.alternatewars.com/BBOW/Ballistics/Ext/Drag_Tables.htm
        // Code generated by vba code written by Jacob Tang

        double mach;
        double cd = 0;

        mach = getMach(getRelativeSpeed(), temp_k);

        //Console.WriteLine("Relative Speed : " + getRelativeSpeed() + " Mach: " + mach);

        if (drag_Model == dragModel.G1)
        {

            if (mach < 0.05) { cd = 0.2558; }
            else if (mach < 0.1) { cd = 0.2487; }
            else if (mach < 0.15) { cd = 0.2413; }
            else if (mach < 0.2) { cd = 0.2344; }
            else if (mach < 0.25) { cd = 0.2278; }
            else if (mach < 0.3) { cd = 0.2214; }
            else if (mach < 0.35) { cd = 0.2155; }
            else if (mach < 0.4) { cd = 0.2104; }
            else if (mach < 0.45) { cd = 0.2061; }
            else if (mach < 0.5) { cd = 0.2032; }
            else if (mach < 0.55) { cd = 0.202; }
            else if (mach < 0.6) { cd = 0.2034; }
            else if (mach < 0.7) { cd = 0.2165; }
            else if (mach < 0.73) { cd = 0.223; }
            else if (mach < 0.75) { cd = 0.2313; }
            else if (mach < 0.78) { cd = 0.2417; }
            else if (mach < 0.8) { cd = 0.2546; }
            else if (mach < 0.83) { cd = 0.2706; }
            else if (mach < 0.85) { cd = 0.2901; }
            else if (mach < 0.88) { cd = 0.3136; }
            else if (mach < 0.9) { cd = 0.3415; }
            else if (mach < 0.93) { cd = 0.3734; }
            else if (mach < 0.95) { cd = 0.4084; }
            else if (mach < 0.98) { cd = 0.4448; }
            else if (mach < 1) { cd = 0.4805; }
            else if (mach < 1.03) { cd = 0.5136; }
            else if (mach < 1.05) { cd = 0.5427; }
            else if (mach < 1.08) { cd = 0.5677; }
            else if (mach < 1.1) { cd = 0.5883; }
            else if (mach < 1.13) { cd = 0.6053; }
            else if (mach < 1.15) { cd = 0.6191; }
            else if (mach < 1.2) { cd = 0.6393; }
            else if (mach < 1.25) { cd = 0.6518; }
            else if (mach < 1.3) { cd = 0.6589; }
            else if (mach < 1.35) { cd = 0.6621; }
            else if (mach < 1.4) { cd = 0.6625; }
            else if (mach < 1.45) { cd = 0.6607; }
            else if (mach < 1.5) { cd = 0.6573; }
            else if (mach < 1.55) { cd = 0.6528; }
            else if (mach < 1.6) { cd = 0.6474; }
            else if (mach < 1.65) { cd = 0.6413; }
            else if (mach < 1.7) { cd = 0.6347; }
            else if (mach < 1.75) { cd = 0.628; }
            else if (mach < 1.8) { cd = 0.621; }
            else if (mach < 1.85) { cd = 0.6141; }
            else if (mach < 1.9) { cd = 0.6072; }
            else if (mach < 1.95) { cd = 0.6003; }
            else if (mach < 2) { cd = 0.5934; }
            else if (mach < 2.05) { cd = 0.5867; }
            else if (mach < 2.1) { cd = 0.5804; }
            else if (mach < 2.15) { cd = 0.5743; }
            else if (mach < 2.2) { cd = 0.5685; }
            else if (mach < 2.25) { cd = 0.563; }
            else if (mach < 2.3) { cd = 0.5577; }
            else if (mach < 2.35) { cd = 0.5527; }
            else if (mach < 2.4) { cd = 0.5481; }
            else if (mach < 2.45) { cd = 0.5438; }
            else if (mach < 2.5) { cd = 0.5397; }
            else if (mach < 2.6) { cd = 0.5325; }
            else if (mach < 2.7) { cd = 0.5264; }
            else if (mach < 2.8) { cd = 0.5211; }
            else if (mach < 2.9) { cd = 0.5168; }
            else if (mach < 3) { cd = 0.5133; }
            else if (mach < 3.1) { cd = 0.5105; }
            else if (mach < 3.2) { cd = 0.5084; }
            else if (mach < 3.3) { cd = 0.5067; }
            else if (mach < 3.4) { cd = 0.5054; }
            else if (mach < 3.5) { cd = 0.504; }
            else if (mach < 3.6) { cd = 0.503; }
            else if (mach < 3.7) { cd = 0.5022; }
            else if (mach < 3.8) { cd = 0.5016; }
            else if (mach < 3.9) { cd = 0.501; }
            else if (mach < 4) { cd = 0.5006; }
            else if (mach < 4.2) { cd = 0.4998; }
            else if (mach < 4.4) { cd = 0.4995; }
            else if (mach < 4.6) { cd = 0.4992; }
            else if (mach < 4.8) { cd = 0.499; }
            else if (mach < 5) { cd = 0.4988; }
            else if (mach > 5) { cd = 0.4988; }

        }

        if (drag_Model == dragModel.G2)
        {
            if (mach < 0.05) { cd = 0.2298; }
            else if (mach < 0.1) { cd = 0.2287; }
            else if (mach < 0.15) { cd = 0.2271; }
            else if (mach < 0.2) { cd = 0.2251; }
            else if (mach < 0.25) { cd = 0.2227; }
            else if (mach < 0.3) { cd = 0.2196; }
            else if (mach < 0.35) { cd = 0.2156; }
            else if (mach < 0.4) { cd = 0.2107; }
            else if (mach < 0.45) { cd = 0.2048; }
            else if (mach < 0.5) { cd = 0.198; }
            else if (mach < 0.55) { cd = 0.1905; }
            else if (mach < 0.6) { cd = 0.1828; }
            else if (mach < 0.65) { cd = 0.1758; }
            else if (mach < 0.7) { cd = 0.1702; }
            else if (mach < 0.75) { cd = 0.1669; }
            else if (mach < 0.78) { cd = 0.1664; }
            else if (mach < 0.8) { cd = 0.1667; }
            else if (mach < 0.83) { cd = 0.1682; }
            else if (mach < 0.85) { cd = 0.1711; }
            else if (mach < 0.88) { cd = 0.1761; }
            else if (mach < 0.9) { cd = 0.1831; }
            else if (mach < 0.93) { cd = 0.2004; }
            else if (mach < 0.95) { cd = 0.2589; }
            else if (mach < 0.98) { cd = 0.3492; }
            else if (mach < 1) { cd = 0.3983; }
            else if (mach < 1.03) { cd = 0.4075; }
            else if (mach < 1.05) { cd = 0.4103; }
            else if (mach < 1.08) { cd = 0.4114; }
            else if (mach < 1.1) { cd = 0.4106; }
            else if (mach < 1.13) { cd = 0.4089; }
            else if (mach < 1.15) { cd = 0.4068; }
            else if (mach < 1.18) { cd = 0.4046; }
            else if (mach < 1.2) { cd = 0.4021; }
            else if (mach < 1.25) { cd = 0.3966; }
            else if (mach < 1.3) { cd = 0.3904; }
            else if (mach < 1.35) { cd = 0.3835; }
            else if (mach < 1.4) { cd = 0.3759; }
            else if (mach < 1.45) { cd = 0.3678; }
            else if (mach < 1.5) { cd = 0.3594; }
            else if (mach < 1.55) { cd = 0.3512; }
            else if (mach < 1.6) { cd = 0.3432; }
            else if (mach < 1.65) { cd = 0.3356; }
            else if (mach < 1.7) { cd = 0.3282; }
            else if (mach < 1.75) { cd = 0.3213; }
            else if (mach < 1.8) { cd = 0.3149; }
            else if (mach < 1.85) { cd = 0.3089; }
            else if (mach < 1.9) { cd = 0.3033; }
            else if (mach < 1.95) { cd = 0.2982; }
            else if (mach < 2) { cd = 0.2933; }
            else if (mach < 2.05) { cd = 0.2889; }
            else if (mach < 2.1) { cd = 0.2846; }
            else if (mach < 2.15) { cd = 0.2806; }
            else if (mach < 2.2) { cd = 0.2768; }
            else if (mach < 2.25) { cd = 0.2731; }
            else if (mach < 2.3) { cd = 0.2696; }
            else if (mach < 2.35) { cd = 0.2663; }
            else if (mach < 2.4) { cd = 0.2632; }
            else if (mach < 2.45) { cd = 0.2602; }
            else if (mach < 2.5) { cd = 0.2572; }
            else if (mach < 2.55) { cd = 0.2543; }
            else if (mach < 2.6) { cd = 0.2515; }
            else if (mach < 2.65) { cd = 0.2487; }
            else if (mach < 2.7) { cd = 0.246; }
            else if (mach < 2.75) { cd = 0.2433; }
            else if (mach < 2.8) { cd = 0.2408; }
            else if (mach < 2.85) { cd = 0.2382; }
            else if (mach < 2.9) { cd = 0.2357; }
            else if (mach < 2.95) { cd = 0.2333; }
            else if (mach < 3) { cd = 0.2309; }
            else if (mach < 3.1) { cd = 0.2262; }
            else if (mach < 3.2) { cd = 0.2217; }
            else if (mach < 3.3) { cd = 0.2173; }
            else if (mach < 3.4) { cd = 0.2132; }
            else if (mach < 3.5) { cd = 0.2091; }
            else if (mach < 3.6) { cd = 0.2052; }
            else if (mach < 3.7) { cd = 0.2014; }
            else if (mach < 3.8) { cd = 0.1978; }
            else if (mach < 3.9) { cd = 0.1944; }
            else if (mach < 4) { cd = 0.1912; }
            else if (mach < 4.2) { cd = 0.1851; }
            else if (mach < 4.4) { cd = 0.1794; }
            else if (mach < 4.6) { cd = 0.1741; }
            else if (mach < 4.8) { cd = 0.1693; }
            else if (mach < 5) { cd = 0.1648; }
            else if (mach > 5) { cd = 0.1648; }

        }

        if (drag_Model == dragModel.G5)
        {
            if (mach < 0.05) { cd = 0.1719; }
            else if (mach < 0.1) { cd = 0.1727; }
            else if (mach < 0.15) { cd = 0.1732; }
            else if (mach < 0.2) { cd = 0.1734; }
            else if (mach < 0.25) { cd = 0.173; }
            else if (mach < 0.3) { cd = 0.1718; }
            else if (mach < 0.35) { cd = 0.1696; }
            else if (mach < 0.4) { cd = 0.1668; }
            else if (mach < 0.45) { cd = 0.1637; }
            else if (mach < 0.5) { cd = 0.1603; }
            else if (mach < 0.55) { cd = 0.1566; }
            else if (mach < 0.6) { cd = 0.1529; }
            else if (mach < 0.65) { cd = 0.1497; }
            else if (mach < 0.7) { cd = 0.1473; }
            else if (mach < 0.75) { cd = 0.1463; }
            else if (mach < 0.8) { cd = 0.1489; }
            else if (mach < 0.85) { cd = 0.1583; }
            else if (mach < 0.88) { cd = 0.1672; }
            else if (mach < 0.9) { cd = 0.1815; }
            else if (mach < 0.93) { cd = 0.2051; }
            else if (mach < 0.95) { cd = 0.2413; }
            else if (mach < 0.98) { cd = 0.2884; }
            else if (mach < 1) { cd = 0.3379; }
            else if (mach < 1.03) { cd = 0.3785; }
            else if (mach < 1.05) { cd = 0.4032; }
            else if (mach < 1.08) { cd = 0.4147; }
            else if (mach < 1.1) { cd = 0.4201; }
            else if (mach < 1.15) { cd = 0.4278; }
            else if (mach < 1.2) { cd = 0.4338; }
            else if (mach < 1.25) { cd = 0.4373; }
            else if (mach < 1.3) { cd = 0.4392; }
            else if (mach < 1.35) { cd = 0.4403; }
            else if (mach < 1.4) { cd = 0.4406; }
            else if (mach < 1.45) { cd = 0.4401; }
            else if (mach < 1.5) { cd = 0.4386; }
            else if (mach < 1.55) { cd = 0.4362; }
            else if (mach < 1.6) { cd = 0.4328; }
            else if (mach < 1.65) { cd = 0.4286; }
            else if (mach < 1.7) { cd = 0.4237; }
            else if (mach < 1.75) { cd = 0.4182; }
            else if (mach < 1.8) { cd = 0.4121; }
            else if (mach < 1.85) { cd = 0.4057; }
            else if (mach < 1.9) { cd = 0.3991; }
            else if (mach < 1.95) { cd = 0.3926; }
            else if (mach < 2) { cd = 0.3861; }
            else if (mach < 2.05) { cd = 0.38; }
            else if (mach < 2.1) { cd = 0.3741; }
            else if (mach < 2.15) { cd = 0.3684; }
            else if (mach < 2.2) { cd = 0.363; }
            else if (mach < 2.25) { cd = 0.3578; }
            else if (mach < 2.3) { cd = 0.3529; }
            else if (mach < 2.35) { cd = 0.3481; }
            else if (mach < 2.4) { cd = 0.3435; }
            else if (mach < 2.45) { cd = 0.3391; }
            else if (mach < 2.5) { cd = 0.3349; }
            else if (mach < 2.6) { cd = 0.3269; }
            else if (mach < 2.7) { cd = 0.3194; }
            else if (mach < 2.8) { cd = 0.3125; }
            else if (mach < 2.9) { cd = 0.306; }
            else if (mach < 3) { cd = 0.2999; }
            else if (mach < 3.1) { cd = 0.2942; }
            else if (mach < 3.2) { cd = 0.2889; }
            else if (mach < 3.3) { cd = 0.2838; }
            else if (mach < 3.4) { cd = 0.279; }
            else if (mach < 3.5) { cd = 0.2745; }
            else if (mach < 3.6) { cd = 0.2703; }
            else if (mach < 3.7) { cd = 0.2662; }
            else if (mach < 3.8) { cd = 0.2624; }
            else if (mach < 3.9) { cd = 0.2588; }
            else if (mach < 4) { cd = 0.2553; }
            else if (mach < 4.2) { cd = 0.2488; }
            else if (mach < 4.4) { cd = 0.2429; }
            else if (mach < 4.6) { cd = 0.2376; }
            else if (mach < 4.8) { cd = 0.2326; }
            else if (mach < 5) { cd = 0.228; }
            else if (mach > 5) { cd = 0.228; }

        }

        if (drag_Model == dragModel.G6)
        {
            if (mach < 0.05) { cd = 0.2553; }
            else if (mach < 0.1) { cd = 0.2491; }
            else if (mach < 0.15) { cd = 0.2432; }
            else if (mach < 0.2) { cd = 0.2376; }
            else if (mach < 0.25) { cd = 0.2324; }
            else if (mach < 0.3) { cd = 0.2278; }
            else if (mach < 0.35) { cd = 0.2238; }
            else if (mach < 0.4) { cd = 0.2205; }
            else if (mach < 0.45) { cd = 0.2177; }
            else if (mach < 0.5) { cd = 0.2155; }
            else if (mach < 0.55) { cd = 0.2138; }
            else if (mach < 0.6) { cd = 0.2126; }
            else if (mach < 0.65) { cd = 0.2121; }
            else if (mach < 0.7) { cd = 0.2122; }
            else if (mach < 0.75) { cd = 0.2132; }
            else if (mach < 0.8) { cd = 0.2154; }
            else if (mach < 0.85) { cd = 0.2194; }
            else if (mach < 0.88) { cd = 0.2229; }
            else if (mach < 0.9) { cd = 0.2297; }
            else if (mach < 0.93) { cd = 0.2449; }
            else if (mach < 0.95) { cd = 0.2732; }
            else if (mach < 0.98) { cd = 0.3141; }
            else if (mach < 1) { cd = 0.3597; }
            else if (mach < 1.03) { cd = 0.3994; }
            else if (mach < 1.05) { cd = 0.4261; }
            else if (mach < 1.08) { cd = 0.4402; }
            else if (mach < 1.1) { cd = 0.4465; }
            else if (mach < 1.13) { cd = 0.449; }
            else if (mach < 1.15) { cd = 0.4497; }
            else if (mach < 1.18) { cd = 0.4494; }
            else if (mach < 1.2) { cd = 0.4482; }
            else if (mach < 1.23) { cd = 0.4464; }
            else if (mach < 1.25) { cd = 0.4441; }
            else if (mach < 1.3) { cd = 0.439; }
            else if (mach < 1.35) { cd = 0.4336; }
            else if (mach < 1.4) { cd = 0.4279; }
            else if (mach < 1.45) { cd = 0.4221; }
            else if (mach < 1.5) { cd = 0.4162; }
            else if (mach < 1.55) { cd = 0.4102; }
            else if (mach < 1.6) { cd = 0.4042; }
            else if (mach < 1.65) { cd = 0.3981; }
            else if (mach < 1.7) { cd = 0.3919; }
            else if (mach < 1.75) { cd = 0.3855; }
            else if (mach < 1.8) { cd = 0.3788; }
            else if (mach < 1.85) { cd = 0.3721; }
            else if (mach < 1.9) { cd = 0.3652; }
            else if (mach < 1.95) { cd = 0.3583; }
            else if (mach < 2) { cd = 0.3515; }
            else if (mach < 2.05) { cd = 0.3447; }
            else if (mach < 2.1) { cd = 0.3381; }
            else if (mach < 2.15) { cd = 0.3314; }
            else if (mach < 2.2) { cd = 0.3249; }
            else if (mach < 2.25) { cd = 0.3185; }
            else if (mach < 2.3) { cd = 0.3122; }
            else if (mach < 2.35) { cd = 0.306; }
            else if (mach < 2.4) { cd = 0.3; }
            else if (mach < 2.45) { cd = 0.2941; }
            else if (mach < 2.5) { cd = 0.2883; }
            else if (mach < 2.6) { cd = 0.2772; }
            else if (mach < 2.7) { cd = 0.2668; }
            else if (mach < 2.8) { cd = 0.2574; }
            else if (mach < 2.9) { cd = 0.2487; }
            else if (mach < 3) { cd = 0.2407; }
            else if (mach < 3.1) { cd = 0.2333; }
            else if (mach < 3.2) { cd = 0.2265; }
            else if (mach < 3.3) { cd = 0.2202; }
            else if (mach < 3.4) { cd = 0.2144; }
            else if (mach < 3.5) { cd = 0.2089; }
            else if (mach < 3.6) { cd = 0.2039; }
            else if (mach < 3.7) { cd = 0.1991; }
            else if (mach < 3.8) { cd = 0.1947; }
            else if (mach < 3.9) { cd = 0.1905; }
            else if (mach < 4) { cd = 0.1866; }
            else if (mach < 4.2) { cd = 0.1794; }
            else if (mach < 4.4) { cd = 0.173; }
            else if (mach < 4.6) { cd = 0.1673; }
            else if (mach < 4.8) { cd = 0.1621; }
            else if (mach < 5) { cd = 0.1574; }
            else if (mach > 5) { cd = 0.1574; }

        }

        if (drag_Model == dragModel.G7)
        {

            if (mach < 0.05) { cd = 0.1197; }
            else if (mach < 0.1) { cd = 0.1196; }
            else if (mach < 0.15) { cd = 0.1194; }
            else if (mach < 0.2) { cd = 0.1193; }
            else if (mach < 0.25) { cd = 0.1194; }
            else if (mach < 0.3) { cd = 0.1194; }
            else if (mach < 0.35) { cd = 0.1194; }
            else if (mach < 0.4) { cd = 0.1193; }
            else if (mach < 0.45) { cd = 0.1193; }
            else if (mach < 0.5) { cd = 0.1194; }
            else if (mach < 0.55) { cd = 0.1193; }
            else if (mach < 0.6) { cd = 0.1194; }
            else if (mach < 0.65) { cd = 0.1197; }
            else if (mach < 0.7) { cd = 0.1202; }
            else if (mach < 0.73) { cd = 0.1207; }
            else if (mach < 0.75) { cd = 0.1215; }
            else if (mach < 0.78) { cd = 0.1226; }
            else if (mach < 0.8) { cd = 0.1242; }
            else if (mach < 0.83) { cd = 0.1266; }
            else if (mach < 0.85) { cd = 0.1306; }
            else if (mach < 0.88) { cd = 0.1368; }
            else if (mach < 0.9) { cd = 0.1464; }
            else if (mach < 0.93) { cd = 0.166; }
            else if (mach < 0.95) { cd = 0.2054; }
            else if (mach < 0.98) { cd = 0.2993; }
            else if (mach < 1) { cd = 0.3803; }
            else if (mach < 1.03) { cd = 0.4015; }
            else if (mach < 1.05) { cd = 0.4043; }
            else if (mach < 1.08) { cd = 0.4034; }
            else if (mach < 1.1) { cd = 0.4014; }
            else if (mach < 1.13) { cd = 0.3987; }
            else if (mach < 1.15) { cd = 0.3955; }
            else if (mach < 1.2) { cd = 0.3884; }
            else if (mach < 1.25) { cd = 0.381; }
            else if (mach < 1.3) { cd = 0.3732; }
            else if (mach < 1.35) { cd = 0.3657; }
            else if (mach < 1.4) { cd = 0.358; }
            else if (mach < 1.5) { cd = 0.344; }
            else if (mach < 1.55) { cd = 0.3376; }
            else if (mach < 1.6) { cd = 0.3315; }
            else if (mach < 1.65) { cd = 0.326; }
            else if (mach < 1.7) { cd = 0.3209; }
            else if (mach < 1.75) { cd = 0.316; }
            else if (mach < 1.8) { cd = 0.3117; }
            else if (mach < 1.85) { cd = 0.3078; }
            else if (mach < 1.9) { cd = 0.3042; }
            else if (mach < 1.95) { cd = 0.301; }
            else if (mach < 2) { cd = 0.298; }
            else if (mach < 2.05) { cd = 0.2951; }
            else if (mach < 2.1) { cd = 0.2922; }
            else if (mach < 2.15) { cd = 0.2892; }
            else if (mach < 2.2) { cd = 0.2864; }
            else if (mach < 2.25) { cd = 0.2835; }
            else if (mach < 2.3) { cd = 0.2807; }
            else if (mach < 2.35) { cd = 0.2779; }
            else if (mach < 2.4) { cd = 0.2752; }
            else if (mach < 2.45) { cd = 0.2725; }
            else if (mach < 2.5) { cd = 0.2697; }
            else if (mach < 2.55) { cd = 0.267; }
            else if (mach < 2.6) { cd = 0.2643; }
            else if (mach < 2.65) { cd = 0.2615; }
            else if (mach < 2.7) { cd = 0.2588; }
            else if (mach < 2.75) { cd = 0.2561; }
            else if (mach < 2.8) { cd = 0.2533; }
            else if (mach < 2.85) { cd = 0.2506; }
            else if (mach < 2.9) { cd = 0.2479; }
            else if (mach < 2.95) { cd = 0.2451; }
            else if (mach < 3) { cd = 0.2424; }
            else if (mach < 3.1) { cd = 0.2368; }
            else if (mach < 3.2) { cd = 0.2313; }
            else if (mach < 3.3) { cd = 0.2258; }
            else if (mach < 3.4) { cd = 0.2205; }
            else if (mach < 3.5) { cd = 0.2154; }
            else if (mach < 3.6) { cd = 0.2106; }
            else if (mach < 3.7) { cd = 0.206; }
            else if (mach < 3.8) { cd = 0.2017; }
            else if (mach < 3.9) { cd = 0.1975; }
            else if (mach < 4) { cd = 0.1935; }
            else if (mach < 4.2) { cd = 0.1861; }
            else if (mach < 4.4) { cd = 0.1793; }
            else if (mach < 4.6) { cd = 0.173; }
            else if (mach < 4.8) { cd = 0.1672; }
            else if (mach < 5) { cd = 0.1618; }
            else if (mach > 5) { cd = 0.1618; }

        }

        if (drag_Model == dragModel.G8)
        {
            if (mach < 0.05) { cd = 0.2105; }
            else if (mach < 0.1) { cd = 0.2104; }
            else if (mach < 0.15) { cd = 0.2104; }
            else if (mach < 0.2) { cd = 0.2103; }
            else if (mach < 0.25) { cd = 0.2103; }
            else if (mach < 0.3) { cd = 0.2103; }
            else if (mach < 0.35) { cd = 0.2103; }
            else if (mach < 0.4) { cd = 0.2103; }
            else if (mach < 0.45) { cd = 0.2102; }
            else if (mach < 0.5) { cd = 0.2102; }
            else if (mach < 0.55) { cd = 0.2102; }
            else if (mach < 0.6) { cd = 0.2102; }
            else if (mach < 0.65) { cd = 0.2102; }
            else if (mach < 0.7) { cd = 0.2103; }
            else if (mach < 0.75) { cd = 0.2103; }
            else if (mach < 0.8) { cd = 0.2104; }
            else if (mach < 0.83) { cd = 0.2104; }
            else if (mach < 0.85) { cd = 0.2105; }
            else if (mach < 0.88) { cd = 0.2106; }
            else if (mach < 0.9) { cd = 0.2109; }
            else if (mach < 0.93) { cd = 0.2183; }
            else if (mach < 0.95) { cd = 0.2571; }
            else if (mach < 0.98) { cd = 0.3358; }
            else if (mach < 1) { cd = 0.4068; }
            else if (mach < 1.03) { cd = 0.4378; }
            else if (mach < 1.05) { cd = 0.4476; }
            else if (mach < 1.08) { cd = 0.4493; }
            else if (mach < 1.1) { cd = 0.4477; }
            else if (mach < 1.13) { cd = 0.445; }
            else if (mach < 1.15) { cd = 0.4419; }
            else if (mach < 1.2) { cd = 0.4353; }
            else if (mach < 1.25) { cd = 0.4283; }
            else if (mach < 1.3) { cd = 0.4208; }
            else if (mach < 1.35) { cd = 0.4133; }
            else if (mach < 1.4) { cd = 0.4059; }
            else if (mach < 1.45) { cd = 0.3986; }
            else if (mach < 1.5) { cd = 0.3915; }
            else if (mach < 1.55) { cd = 0.3845; }
            else if (mach < 1.6) { cd = 0.3777; }
            else if (mach < 1.65) { cd = 0.371; }
            else if (mach < 1.7) { cd = 0.3645; }
            else if (mach < 1.75) { cd = 0.3581; }
            else if (mach < 1.8) { cd = 0.3519; }
            else if (mach < 1.85) { cd = 0.3458; }
            else if (mach < 1.9) { cd = 0.34; }
            else if (mach < 1.95) { cd = 0.3343; }
            else if (mach < 2) { cd = 0.3288; }
            else if (mach < 2.05) { cd = 0.3234; }
            else if (mach < 2.1) { cd = 0.3182; }
            else if (mach < 2.15) { cd = 0.3131; }
            else if (mach < 2.2) { cd = 0.3081; }
            else if (mach < 2.25) { cd = 0.3032; }
            else if (mach < 2.3) { cd = 0.2983; }
            else if (mach < 2.35) { cd = 0.2937; }
            else if (mach < 2.4) { cd = 0.2891; }
            else if (mach < 2.45) { cd = 0.2845; }
            else if (mach < 2.5) { cd = 0.2802; }
            else if (mach < 2.6) { cd = 0.272; }
            else if (mach < 2.7) { cd = 0.2642; }
            else if (mach < 2.8) { cd = 0.2569; }
            else if (mach < 2.9) { cd = 0.2499; }
            else if (mach < 3) { cd = 0.2432; }
            else if (mach < 3.1) { cd = 0.2368; }
            else if (mach < 3.2) { cd = 0.2308; }
            else if (mach < 3.3) { cd = 0.2251; }
            else if (mach < 3.4) { cd = 0.2197; }
            else if (mach < 3.5) { cd = 0.2147; }
            else if (mach < 3.6) { cd = 0.2101; }
            else if (mach < 3.7) { cd = 0.2058; }
            else if (mach < 3.8) { cd = 0.2019; }
            else if (mach < 3.9) { cd = 0.1983; }
            else if (mach < 4) { cd = 0.195; }
            else if (mach < 4.2) { cd = 0.189; }
            else if (mach < 4.4) { cd = 0.1837; }
            else if (mach < 4.6) { cd = 0.1791; }
            else if (mach < 4.8) { cd = 0.175; }
            else if (mach < 5) { cd = 0.1713; }
            else if (mach > 5) { cd = 0.1713; }

        }

        if (drag_Model == dragModel.GS)
        {
            if (mach < 0.05) { cd = 0.4689; }
            else if (mach < 0.1) { cd = 0.4717; }
            else if (mach < 0.15) { cd = 0.4745; }
            else if (mach < 0.2) { cd = 0.4772; }
            else if (mach < 0.25) { cd = 0.48; }
            else if (mach < 0.3) { cd = 0.4827; }
            else if (mach < 0.35) { cd = 0.4852; }
            else if (mach < 0.4) { cd = 0.4882; }
            else if (mach < 0.45) { cd = 0.492; }
            else if (mach < 0.5) { cd = 0.497; }
            else if (mach < 0.55) { cd = 0.508; }
            else if (mach < 0.6) { cd = 0.526; }
            else if (mach < 0.65) { cd = 0.559; }
            else if (mach < 0.7) { cd = 0.592; }
            else if (mach < 0.75) { cd = 0.6258; }
            else if (mach < 0.8) { cd = 0.661; }
            else if (mach < 0.85) { cd = 0.6985; }
            else if (mach < 0.9) { cd = 0.737; }
            else if (mach < 0.95) { cd = 0.7757; }
            else if (mach < 1) { cd = 0.814; }
            else if (mach < 1.05) { cd = 0.8512; }
            else if (mach < 1.1) { cd = 0.887; }
            else if (mach < 1.15) { cd = 0.921; }
            else if (mach < 1.2) { cd = 0.951; }
            else if (mach < 1.25) { cd = 0.974; }
            else if (mach < 1.3) { cd = 0.991; }
            else if (mach < 1.35) { cd = 0.999; }
            else if (mach < 1.4) { cd = 1.003; }
            else if (mach < 1.45) { cd = 1.006; }
            else if (mach < 1.5) { cd = 1.008; }
            else if (mach < 1.55) { cd = 1.009; }
            else if (mach < 1.6) { cd = 1.009; }
            else if (mach < 1.65) { cd = 1.009; }
            else if (mach < 1.7) { cd = 1.009; }
            else if (mach < 1.75) { cd = 1.008; }
            else if (mach < 1.8) { cd = 1.007; }
            else if (mach < 1.85) { cd = 1.006; }
            else if (mach < 1.9) { cd = 1.004; }
            else if (mach < 1.95) { cd = 1.0025; }
            else if (mach < 2) { cd = 1.001; }
            else if (mach < 2.05) { cd = 0.999; }
            else if (mach < 2.1) { cd = 0.997; }
            else if (mach < 2.15) { cd = 0.9956; }
            else if (mach < 2.2) { cd = 0.994; }
            else if (mach < 2.25) { cd = 0.9916; }
            else if (mach < 2.3) { cd = 0.989; }
            else if (mach < 2.35) { cd = 0.9869; }
            else if (mach < 2.4) { cd = 0.985; }
            else if (mach < 2.45) { cd = 0.983; }
            else if (mach < 2.5) { cd = 0.981; }
            else if (mach < 2.55) { cd = 0.979; }
            else if (mach < 2.6) { cd = 0.977; }
            else if (mach < 2.65) { cd = 0.975; }
            else if (mach < 2.7) { cd = 0.973; }
            else if (mach < 2.75) { cd = 0.971; }
            else if (mach < 2.8) { cd = 0.969; }
            else if (mach < 2.85) { cd = 0.967; }
            else if (mach < 2.9) { cd = 0.965; }
            else if (mach < 2.95) { cd = 0.963; }
            else if (mach < 3) { cd = 0.961; }
            else if (mach < 3.05) { cd = 0.9589; }
            else if (mach < 3.1) { cd = 0.957; }
            else if (mach < 3.15) { cd = 0.9555; }
            else if (mach < 3.2) { cd = 0.954; }
            else if (mach < 3.25) { cd = 0.952; }
            else if (mach < 3.3) { cd = 0.95; }
            else if (mach < 3.35) { cd = 0.9485; }
            else if (mach < 3.4) { cd = 0.947; }
            else if (mach < 3.45) { cd = 0.945; }
            else if (mach < 3.5) { cd = 0.943; }
            else if (mach < 3.55) { cd = 0.9414; }
            else if (mach < 3.6) { cd = 0.94; }
            else if (mach < 3.65) { cd = 0.9385; }
            else if (mach < 3.7) { cd = 0.937; }
            else if (mach < 3.75) { cd = 0.9355; }
            else if (mach < 3.8) { cd = 0.934; }
            else if (mach < 3.85) { cd = 0.9325; }
            else if (mach < 3.9) { cd = 0.931; }
            else if (mach < 3.95) { cd = 0.9295; }
            else if (mach < 4) { cd = 0.928; }
            else if (mach > 4) { cd = 0.928; }

        }

        return cd;
    }


    public double GetStabilityFactor()
    {

        //Improved Miller Formula

        unitConversion();

        double stabilityFactor;

        stabilityFactor = 30 * this.grains / (Math.Pow(this.twist_Calibers, 2) * Math.Pow(this.bullet_Dia_Inch, 3) * this.calibers * (1 + Math.Pow(this.calibers, 2)));
        stabilityFactor = stabilityFactor * Math.Pow((this.muzzle_Vel_Fps / 2800f), (1/3));
        stabilityFactor = stabilityFactor * ((this.temp_F + 460) / (519)) * (29.92 / this.baro);

        return stabilityFactor;
    }

    private void unitConversion()
    {

        this.muzzle_Vel_Fps = this.muzzle_Velocity * 3.28084;
        this.baro = this.pressure * 0.0002953;
        this.temp_F = ((this.temp_k - 273.15) * 1.8) + 32;

    }

    private double[] vectorNormalize(double[] input)
    {

        double[] outpout = { 0, 0, 0 };

        outpout[0] = input[0] / vectorlength(input);
        outpout[1] = input[1] / vectorlength(input);
        outpout[2] = input[2] / vectorlength(input);

        return outpout;
    }

    private double vectorlength(double[] input)
    {
        return Math.Sqrt(Math.Pow(input[0], 2) + Math.Pow(input[1], 2) + Math.Pow(input[2], 2)); ;
    }

    private double[] vectorOperation(double[] input1, double[] input2, string _operator)
    {

        double[] outpout = { 0, 0, 0 };

        if (_operator == "+")
        {
            output[0] = input1[0] + input2[0];
            output[1] = input1[1] + input2[1];
            output[2] = input1[2] + input2[2];
        }
        else if (_operator == "-")
        {
            output[0] = input1[0] - input2[0];
            output[1] = input1[1] - input2[1];
            output[2] = input1[2] - input2[2];
        }
        else if (_operator == "*")
        {
            output[0] = input1[0] * input2[0];
            output[1] = input1[1] * input2[1];
            output[2] = input1[2] * input2[2];
        }

        //Console.WriteLine("Input1 : " + input1[0] + " , " + input1[1] + " , " + +input1[2]);
        //Console.WriteLine("Input2 : " + input2[0] + " , " + input2[1] + " , " + +input2[2]);
        //Console.WriteLine("Output : " + outpout[0] + " , " + outpout[1] + " , "+ +outpout[2]);

        return output;
    }









}


