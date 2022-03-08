import msvcrt
import time
import Partical


_h: float = 5.0
_lowBound = 0
_upBound = 200
_leftBound = 0
_rightBound = 200
_deltaTime = 0.125 * _h **2 * Partical._initDensity / Partical._viscosity


particalList : list[Partical.Partical] = []



def start():
  print("set particals")
  putParticals()

  for i in range(100000):
    print("step ", i)
    time_start = time.time()
    oneSetp(_deltaTime)
    print("cost time: ", time.time() - time_start)

    key = msvcrt.getch()
    if key == ' ': break





def putParticals():
  for x in range(1, 51):
    for y in range(1, 51):
      particalList.append(Partical.Partical(x, y, 0.01))



#1. compute location of every particals
#2. compute density of every particals
#3. compute pressrue and Viscous coefficient of every particals
#4. compute velocity of every particals
#5. continue step 1 
def oneSetp(deltaTime: float):
  #2. compute density and pressrue of every particals
  for point in particalList:
    dpdt = point.computeDensity(particalList)
    point.density += dpdt * deltaTime

  #3. compute pressrue and Viscous coefficient of every particals
  for point in particalList:
    point.computePressrue()
    point.computeViscous(particalList)

  #4. compute velocity of every particals
  for point in particalList:
    dvxdt, dvydt = point.computeVelocity(particalList)
    newVelX = point.velX + dvxdt * deltaTime
    newVelY = point.velY + dvydt * deltaTime
    newPosX = (newVelX + point.velX) / 2 * deltaTime
    newPosY = (newVelY + point.velY) / 2 * deltaTime

    #check boundary
    if newPosX < _leftBound:
      newPosX = _leftBound
      if newVelX < 0: 
        newVelX = 0

    if newPosX > _rightBound:
      newPosX = _rightBound
      if newVelX > 0: 
        newVelX = 0

    if newPosY > _upBound:
      newPosY = _upBound
      if newVelY > 0: 
        newVelY = 0

    if newPosY < _lowBound:
      newPosY = _lowBound
      if newVelY < 0: 
        newVelY = 0

    point.posX = newPosX
    point.posY = newPosY
    point.velX = newVelX
    point.velY = newVelY



start()