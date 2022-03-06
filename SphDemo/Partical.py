import math
from typing import List



__initDensity = 100
__viscosity : float= 100



class Partical(object):
  posX = 0
  posY = 0
  velX = 0
  velY = 0
  mass = 100
  density = __initDensity
  pressure = 0



  def __init__(self, x, y):
    self.posX = x
    self.posY = y



  def computeDensity(self, neigborList: list['Partical']) -> float:
    dpdt = 0

    for other in neigborList:
      velXDiff = self.velX - other.velX
      velYDiff = self.velY - other.velY

      dwdx, dwdy = self.kenelFunction2derivative(other);
      dpdtofNeigbor = self.density * other.mass / other.density \
       * (velXDiff * dwdx + velYDiff * dwdy)
      dpdt += dpdtofNeigbor

    return dpdt



  def computePressure(self, neigborList: list['Partical']) -> float:
    #the important is to confine the direction of the pressure
    #this use the pressure gradient equation from the papar of MPS
    #and plus the power of mass 

    gradientX = 0
    gradientY = 0

    for other in neigborList:
      w = self.kenelFunction(other)
      distance = math.sqrt( (self.posX - other.posX) **2 + (self.posY - other.posY) **2 )
      temp = other.mass / other.density * w * (self.pressure - other.pressure) / distance
      gradientX += temp * (self.posX - other.posX)
      gradientY += temp * (self.posY - other.posY)

    gradient = math.sqrt(gradientY ** 2 + gradientY **2)
    gradientX /= gradient
    gradientY /= gradient

    B = 32
    press = B * ((self.density / __initDensity) ** 7 - 1)
    return press * gradientX, press * gradientY
  


  def computeVelocity(self, neigborList: list['Partical']):
    dvdtX = 0;
    dvdtY = 0;

    for other in neigborList:
      velXDiff = self.velX - other.velX
      velYDiff = self.velY - other.velY

      dwdx, dwdy = self.kenelFunction2derivative(other);
      dpdtofNeigbor = self.density * other.mass / other.density \
       * (velXDiff * dwdx + velYDiff * dwdy)
      dpdt += dpdtofNeigbor

    return dvdtX, dvdtY

