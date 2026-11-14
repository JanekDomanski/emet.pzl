## About
Library is a replacement of standard bending development formula for Solid Edge. In material properties You can assign emet.pzl and make flat patterns compatible with Your bending tables without changing bending radii in original 3D models. Works even if 3D-model radii are wrong and if 3D-model thickness is wrong. Formulas are based on compensated PZL-zone length and fake K-factors.
Library is developed and extensively tested at the Polish "emet" company which makes sheet metal equipment. 

## Installation
Library must be registered in Windows. Here is command:
%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\regasm /codebase "c:\path\to\emet.dll"
Library requires bending table (emet.xlsx) in the same directory.

## Maths
### Variable definitions
BD: bend deduction  
PZL: Plastic zone length (aka bend allowance)  
…m: value from 3D model  
…t: value from bending table

### Axioms
![image](resources/bending_202611100817.png)

K=(Rn-R)/T *[ANSI]*  
PZL=α×Rn  
σ=2×S+2×F-BD  
σ=PZL+2×F

Rm≠Rt ∨ Rm=Rt  
Tm≠Tt ∨ Tm=Tt

### Derived equations
σ=2×S+2×F-BD=PZL+2×F  
2×S+2×F-2×F-BD=PZL  
2×S-BD=PZL ⇒ **BD=2×S-PZL**  

S/Re=Tan(α/2) ⇒ S=Re×Tan(α/2)  
S=Re×Tan(α/2), Re=R+T ⇒ **S=(R+T)×Tan(α/2)**  

PZL=α×Rn, Rn=R+T×K ⇒ **PZL=α×(R+T×K)**  
PZL=α×R+α×K×T  
PZL-α×R=α×K×T | /(α×T)  
K=(PZL-α×R)/(α×T)  
**K=(PZL/α-R)/T**

### Hems
![image](resources/bending_hems.png)
Closed flat hems are calculated using these simplifications:  
α=Π *[bending angle for hems: always 180° = Π radians]*  
Tm=Tt *[always choose material thickness from table, omit 3D model thickness]*  
Rm=Rt=G/2 *[G: default hem gap, in SolidEdge = 0.004 mm, omit 3D model hem gap]*


### PZL Compensation
- Reducing 3D Model PZL to 0
- Calculating PZL that compensates radius difference (PZLc)

Dmopzl: Deduction from 3D model with PZL reduced to 0  
Dmopzl=2×Sm  
BDt=2×St-PZLt  
PZLc=BDmopzl-BDt  
PZLc=2×Sm-(2×St-PZLt)  
PZLc=2×Sm-2×St+PZLt  
PZLc=2×(Rm+Tm)×Tan(α/2)-2×(Rt+Tt)×Tan(α/2)+α×(Rt+Tt×K)

**PZLc=2×Tan(α/2)(Rm-Rt+Tm-Tt)+α×(Rt+Tt×K)**

### Fake K-factor
When this factor is used in the standard bend-development formula we've got good flat pattern even if 3D Model thickness or 3D model radii differs from bending tables. K-factor results from compensated PZL which can be 0. So fake K-factor can be outside reasonable range (0,1>  

K=(PZL/α-R)/T  
Kfake=(PZLc/α-Rm)/Tm  
If we use previously calculated PZLc, then we've got PZLc-independent formula:  
Kfake=((2×Tan(α/2)(Rm-Rt+Tm-Tt)+α×(Rt+Tt×Kt))/α-Rm)/Tm   
**Kfake=(2×Tan(α/2)/α×(Rm-Rt+Tm-Tt)+Rt+Tt×Kt-Rm)/Tm**  
PZLc≥0 ⇒ Kfake≥(0/α-Rm)/Tm ⇒ Kfake≥-Rm/Tm

## Disclaimer
This project is an independent project and is not affiliated with, endorsed by, or officially supported by Siemens or Solid Edge. Solid Edge is a trademark of Siemens and/or its affiliates.
