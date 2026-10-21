"""Reproduce legal crate solutions for Level1, Level2 and Level3 (no Unity writes)."""
from collections import deque
from pathlib import Path
import itertools, json
D=((1,0),(-1,0),(0,1),(0,-1))
def add(a,b): return a[0]+b[0],a[1]+b[1]
def sub(a,b): return a[0]-b[0],a[1]-b[1]
def room(x0,x1,y0,y1):return {(x,y) for x in range(x0,x1+1) for y in range(y0,y1+1)}
def ring(x0,x1,y0,y1):return room(x0,x1,y0,y1)-room(x0+1,x1-1,y0+1,y1-1)
def solve(level):
 if level==2:return solve_detour()
 if level<3:
  conv=lambda p:(p[0],-p[1])
  goalrow=9 if level==1 else 12
  floor=room(1,5,1,5)|room(8,12,1,5)|room(8,12,goalrow-1,goalrow+1)
  if level==2:floor|={(5,6),(9,6),(4,7),(9,8),(7,5),(7,6),(7,8)}|room(5,9,7,7)
  floor={conv(p) for p in floor};walls={conv(p) for p in ring(7,13,goalrow-2,goalrow+2)}
  player=conv((2,5 if level==1 else 4))
  regular={conv(p) for p in ([(2,4),(9,3)] if level==1 else [(3,3),(3,4)])}
  targets={conv(p) for p in ([(2,2),(11,3)] if level==1 else [(11,2),(11,4)])}
  bridges=set();holes=set()
 else:
  void={(x,y) for x in (4,10) for y in (5,6,8,9)}|{(x,10) for x in (5,6,8,9)}|{(x,4) for x in range(5,10)}
  floor=({(x,y) for x in range(15) for y in range(15) if 5<=x<=9 or 5<=y<=9})-void
  walls=ring(4,10,-1,4);player=(7,7);regular={(1,7),(13,7),(8,13)};targets={(5,9),(9,9),(9,5)}
  bridges={(1,8),(6,13)};holes={(4,7),(10,7)}
 space=room(-2,17,-17,17)-walls
 def walk(p,occupied):
  q=deque([p]);prev={p:None}
  while q:
   v=q.popleft()
   for d in D:
    n=add(v,d)
    if n in space and n not in occupied and n not in prev:prev[n]=v;q.append(n)
  return prev
 def route(p,b,goal,others,filled,bridge):
  start=(p,b);q=deque([start]);prev={start:None}
  while q:
   state=q.popleft();p,b=state
   if b==goal:
    chain=[]
    while state!=start:
     old,stand,d=prev[state];chain.append((old,stand,d));state=old
    return list(reversed(chain))
   reachable=walk(p,others|{b})
   for d in D:
    stand=sub(b,d);dest=add(b,d)
    if stand not in reachable or stand not in floor or stand in holes-filled:continue
    if dest not in floor or dest in walls or dest in others:continue
    if dest in holes-filled and (not bridge or dest!=goal):continue
    ns=(b,dest)
    if ns not in prev:prev[ns]=(state,stand,d);q.append(ns)
  return None
 for assignments in itertools.permutations(sorted(holes)):
  p=player;remaining=set(bridges);filled=set();actions=[];pushes=0
  def apply(chain,others):
   nonlocal p,pushes
   for (oldp,b),stand,d in chain:
    prev=walk(oldp,others|{b});cur=stand;path=[]
    while cur!=oldp:path.append(cur);cur=prev[cur]
    p=oldp
    for n in reversed(path):
     # Gas walking avoids unintended drain teleports; all pushes still use Solid.
     actions.append({'dx':n[0]-p[0],'dy':n[1]-p[1],'state':2});p=n
    actions.append({'dx':d[0],'dy':d[1],'state':0});p=b;pushes+=1
  for b,g in zip(sorted(bridges),assignments):
   others=regular|(remaining-{b});chain=route(p,b,g,others,filled,True)
   if chain is None:break
   apply(chain,others);remaining.remove(b);filled.add(g)
  else:
   checkpoint=p,actions[:],pushes
   for crates in itertools.permutations(sorted(regular)):
    for goals in itertools.permutations(sorted(targets)):
     p,actions,pushes=checkpoint[0],checkpoint[1][:],checkpoint[2];current=set(regular)
     for b,g in zip(crates,goals):
      others=current-{b};chain=route(p,b,g,others,filled,False)
      if chain is None:break
      apply(chain,others);current.remove(b);current.add(g)
     else:return {'actions':actions,'pushes':pushes}
 raise RuntimeError(f'Level{level} has no solution')
def solve_detour():
 """A full solution using temperature tiles only; -1 means never press E."""
 floor=room(1,5,1,5)|room(8,12,1,5)|room(8,12,11,13)|room(5,9,7,7)|room(3,5,7,9)
 floor|={(5,6),(9,6),(4,7),(9,8),(7,5),(7,6),(7,8),(8,8),(3,10)}
 walls=ring(7,13,10,14);space=room(0,14,0,15)-walls
 heater=(3,2);cooler=(3,10);drains={(12,5):(12,12),(12,12):(12,5)}
 p=(2,4);state=0;crates={(3,3),(3,4)};actions=[];pushes=0;heats=0;cools=0
 def step(d,n,s,teleport=False):
  # Convert drawing rows to Unity y-up coordinates.
  return {'dx':d[0],'dy':-d[1],'state':-1,'afterState':s,'teleport':teleport,'ex':n[0],'ey':-n[1]}
 def walk(to,wanted,allow_drains=False):
  nonlocal p,state,heats,cools
  start=(p,state);q=deque([start]);prev={start:None};found=None
  while q:
   pos,form=q.popleft()
   if pos==to and (wanted is None or form==wanted):found=(pos,form);break
   for d in D:
    n=add(pos,d)
    if n not in space or n in crates or (form!=2 and n not in floor):continue
    if not allow_drains and n in drains:continue
    ns=min(form+1,2) if n==heater else max(form-1,0) if n==cooler else form
    teleport=n in drains and ns==1
    if teleport:n=drains[n]
    key=(n,ns)
    if key not in prev:prev[key]=((pos,form),step(d,n,ns,teleport));q.append(key)
  if found is None:raise RuntimeError(f'No temperature-only route from {p,state} to {to,wanted}, crates={crates}')
  path=[];cur=found
  while cur!=start:
   old,action=prev[cur];path.append(action);cur=old
  for a in reversed(path):
   actions.append(a)
   n=(a['ex'],-a['ey'])
   if n==heater:heats+=1
   if n==cooler:cools+=1
  p,state=found
 chains=[((3,3),[(4,3),(5,3),(5,4),(5,5),(5,6),(5,7),(6,7),(7,7),(8,7),(9,7),(9,6),(9,5),(9,4),(10,4),(11,4),(11,3),(11,2)]),
         ((3,4),[(4,4),(5,4),(5,5),(5,6),(5,7),(6,7),(7,7),(8,7),(9,7),(9,6),(9,5),(9,4),(10,4),(11,4)])]
 for crate,chain in chains:
  for dest in chain:
   d=sub(dest,crate);stand=sub(crate,d)
   walk(stand,0)
   assert dest in floor and dest not in crates and dest not in walls
   crates.remove(crate);crates.add(dest)
   actions.append(step(d,crate,0));p=crate;state=0;crate=dest;pushes+=1
 assert crates=={(11,2),(11,4)}
 objective_moves=len(actions)
 walk((10,12),1,allow_drains=True)
 assert heats>=1 and cools>=1 and any(a['teleport'] for a in actions)
 print(f'Detour temperature-only route: {heats} heater entries, {cools} cooler entries, {pushes} pushes, {len(actions)} moves including drain and goal.')
 return {'actions':actions,'pushes':pushes,'completeRoute':True,'objectiveMoves':objective_moves}

def solve_gag():
 """Solve Level2.5 with temperature triggers, two bridges and movable targets."""
 floor=room(1,5,1,5)|room(8,12,1,5)|room(1,5,10,12)|room(8,12,11,13)|room(4,9,7,7)|room(4,7,8,8)
 floor|=room(10,10,6,8)
 floor|={(5,6),(9,6),(7,5),(7,6),(9,8),(4,8),(4,9)}
 walls=ring(7,13,10,14);space=room(0,14,0,15)-walls;holes={(4,8),(4,9)};filled=set()
 heater=(3,2);cooler=(1,12);drains={(12,5):(12,12),(12,12):(12,5)}
 crates={(3,3),(3,4)};bridges={(3,11),(4,11)};targets={(11,2),(11,4)}
 p=(2,4);form=0;actions=[];bridge_pushes=0;target_pushes=0
 def action(d,n,state,teleport=False):
  return {'dx':d[0],'dy':-d[1],'state':-1,'afterState':state,'teleport':teleport,'ex':n[0],'ey':-n[1]}
 def walk(to,wanted,allow_drains=False):
  nonlocal p,form
  start=(p,form);q=deque([start]);prev={start:None};found=None
  while q:
   pos,state=q.popleft()
   if pos==to and (wanted is None or state==wanted):found=(pos,state);break
   for d in D:
    n=add(pos,d)
    if n not in space or n in crates|bridges|targets:continue
    if state!=2 and (n not in floor or n in holes-filled):continue
    if n in drains and not allow_drains:continue
    ns=min(state+1,2) if n==heater else max(state-1,0) if n==cooler else state
    teleport=n in drains and ns==1
    if teleport:n=drains[n]
    key=n,ns
    if key not in prev:prev[key]=((pos,state),action(d,n,ns,teleport));q.append(key)
  if found is None:raise RuntimeError(f'Gag route blocked: {p,form} -> {to,wanted}')
  path=[];cur=found
  while cur!=start:
   old,a=prev[cur];path.append(a);cur=old
  actions.extend(reversed(path));p,form=found
 for block,chain in [((4,11),[(4,10),(4,9)]),((3,11),[(4,11),(4,10),(4,9),(4,8)])]:
  for dest in chain:
   d=sub(dest,block);walk(sub(block,d),0)
   assert dest in floor and dest not in crates|bridges
   bridges.remove(block)
   if dest in holes-filled:filled.add(dest)
   else:bridges.add(dest)
   actions.append(action(d,block,0));p=block;form=0;block=dest;bridge_pushes+=1
 assert filled==holes and not bridges
 for marker,chain in [((11,2),[(10,2),(9,2),(9,3),(9,4),(9,5),(9,6),(9,7),(8,7),(7,7),(6,7),(5,7),(5,6),(5,5),(5,4),(5,3),(4,3),(3,3)]),
                      ((11,4),[(10,4),(9,4),(9,5),(9,6),(9,7),(8,7),(7,7),(6,7),(5,7),(5,6),(5,5),(5,4),(4,4),(3,4)])]:
  for dest in chain:
   d=sub(dest,marker);walk(sub(marker,d),2)
   assert dest in floor and dest not in targets and dest not in holes-filled
   targets.remove(marker);targets.add(dest)
   actions.append(action(d,marker,2));p=marker;form=2;marker=dest;target_pushes+=1
 assert targets==crates
 objective_moves=len(actions)
 walk((10,12),1,allow_drains=True)
 print(f'Gag: {bridge_pushes} bridge pushes, {target_pushes} target pushes, {len(actions)} moves; temperature-only solution.')
 return {'actions':actions,'pushes':bridge_pushes+target_pushes,'completeRoute':True,'objectiveMoves':objective_moves}

if __name__=='__main__':
 levels=[solve(1),solve(2),solve_gag(),solve(3)]
 Path(__file__).with_name('level1-solution.json').write_text(json.dumps({'levels':levels}))
 for i,l in zip((1,2,2.5,3),levels):print(f'Level{i}: {l["pushes"]} pushes, {len(l["actions"])} moves to cover all targets.')
