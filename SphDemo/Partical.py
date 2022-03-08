import math
import kenelFcunction
from typing import List



#1. compute location of every particals
#2. compute density and pressrue of every particals
#3. compute pressure, Viscous coefficient of every particals
#4. compute velocity of every particals according to the total force of every particals
#5. continue step 1 

__c = 200
__initDensity : float = 100
__viscosity : float = 100
__gravityY : float = -9.8



class Partical(object):
  posX = 0
  posY = 0
  velX = 0
  velY = 0
  accX = 0
  accY = 0
  mass = 100
  density = __initDensity


  pressure = 0
  tauXX, tauXY, tauYY, tauYX = 0


  def __init__(self, x, y):
    self.posX = x
    self.posY = y



  def computeDensity(self, neigborList: list['Partical']) -> float:
    dpdt = 0

    for point in neigborList:
      dwdx, dwdy = kenelFcunction.kenelDerivative(self, point)
      dpdt += self.density * point.mass / point.density \
       * ((self.velX - point.velX) * dwdx + (self.velY - point.velY) * dwdy)

    return dpdt



  def computePressrue(self):
    B = 32
    self.pressure = B * ((self.density / __initDensity) ** 7 - 1)
    #self.pressure = self.density * __c **2


  def computeViscous(self, neigborList: list['Partical']):
    dvdx = 0
    dvdy = 0
    for point in neigborList:
      dwdx, dwdy = kenelFcunction.kenelDerivative(self, point)
      dvdx += point.mass / point.density * point.velX * dwdx
      dvdy += point.mass / point.density * point.vely * dwdy

    self.tauXX = __viscosity * (2 * dvdx - 2/3 * (dvdx + dvdy))
    self.tauYY = __viscosity * (2 * dvdy - 2/3 * (dvdx + dvdy))
    self.tauXY = __viscosity * (dvdx + dvdy)
    self.tauYX = __viscosity * (dvdx + dvdy)



  def computeVelocity(self, neigborList: list['Partical']):
    dvxdt = 0;
    dvydt = 0;

    for point in neigborList:
      dwdx, dwdy = kenelFcunction.kenelDerivative(self, point)
      temp = point.density / (self.density * point.density)

      dvxdt += - temp * (self.pressure + point.pressure) * dwdx
      dvxdt += temp * (self.tauXX + point.tauXX) * dwdx
      dvxdt += temp * (self.tauXY + point.tauXY) * dwdy

      dvydt += - temp * (self.pressure + point.pressure) * dwdy
      dvydt += temp * (self.tauYY + point.tauYY) * dwdy
      dvydt += temp * (self.tauYX + point.tauYX) * dwdx

    return dvxdt, dvydt
  


  #the followings are deprecated
  def computePressureGradient(self, neigborList: list['Partical']) -> float:
    #the important is to confine the direction of the pressure
    #this use the pressure gradient equation from the papar of MPS
    #and plus the power of mass 
    gradientX = 0
    gradientY = 0

    for point in neigborList:
      w = self.kenelFunction(point)
      distance = math.sqrt( (self.posX - point.posX) **2 + (self.posY - point.posY) **2 )
      temp = point.mass / point.density * w * (self.pressure - point.pressure) / distance
      gradientX += temp * (self.posX - point.posX)
      gradientY += temp * (self.posY - point.posY)

    return gradientX, gradientY



  def computeViscousForceGradient(self, neigborList: list['Partical']):
    gradientX = 0
    gradientY = 0

    for point in neigborList:
      d2wdx2, d2wdy2, d2wdxdy = kenelFcunction.kenelDerivative2Order(self, point)

      #inclue the normal stress and the shear stress
      vfX = __viscosity * point.mass / point.density \
        * (point.velX * d2wdxdy + point.velY * d2wdy2 \
        + 4/3 * point.velX * d2wdx2 - 2/3 * point.velY * d2wdxdy)
      vfY = __viscosity * point.mass / point.density \
        * (point.velY * d2wdxdy + point.velX * d2wdx2 \
        + 4/3 * point.velY * d2wdy2 - 2/3 * point.velX * d2wdxdy)
      gradientX += vfX
      gradientY += vfY

    return gradientX, gradientY



  def computeTotalForceGradient(self, neigborList: list['Partical']):
    pressureGradientX, pressureGradientY = self.computePressureGradient(neigborList)
    viscousForceGradientX, viscousForceGradientY = self.computeViscousForceGradient(neigborList)

    self.beForceGradientX = -pressureGradientX + viscousForceGradientX
    self.beForceGradientY = -pressureGradientY + viscousForceGradientY
