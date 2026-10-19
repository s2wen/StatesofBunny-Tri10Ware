"""Find and export a legal Level1 solution for the Unity Play Mode verifier."""
from collections import deque
from pathlib import Path
import itertools,json
board={(x,y) for x in range(15) for y in range(15) if 5<=x<=9 or 5<=y<=9}
walls={(x,y) for x in (4,10) for y in (5,6,8,9)}|{(x,10) for x in (5,6,8,9)}|{(x,4) for x in range(5,10)}
holes={(4,7),(10,7)};targets=((5,9),(9,9),(9,5));directions=((1,0),(-1,0),(0,1),(0,-1))
def add(a,b):return a[0]+b[0],a[1]+b[1]
def sub(a,b):return a[0]-b[0],a[1]-b[1]
def walk(p,occupied):
 q=deque([p]);prev={p:None}
 while q:
  v=q.popleft()
  for d in directions:
   n=add(v,d)
   if n in board and n not in occupied and n not in prev:prev[n]=v;q.append(n)
 return prev

def route_crate(player,crate,goal,others,filled,bridge):
 start=(player,crate);q=deque([start]);prev={start:None}
 while q:
  state=q.popleft();p,b=state
  if b==goal:
   chain=[]
   while state!=start:
    old,stand,d=prev[state];chain.append((old,stand,d));state=old
   return list(reversed(chain))
  reachable=walk(p,others|{b})
  for d in directions:
   stand=sub(b,d);dest=add(b,d)
   if stand not in reachable or stand in walls or stand in holes-filled:continue
   if dest not in board or dest in walls or dest in others:continue
   if dest in holes-filled and (not bridge or dest!=goal):continue
   ns=(b,dest)
   if ns not in prev:prev[ns]=(state,stand,d);q.append(ns)
 return None

def solve():
 for assignments in itertools.permutations(holes):
  p=(7,7);regular={(1,7),(13,7),(8,13)};bridges={(1,8),(6,13)};filled=set();actions=[];pushes=0
  def apply(chain,others):
   nonlocal p,pushes
   for (oldp,b),stand,d in chain:
    prev=walk(oldp,others|{b});cur=stand;path=[]
    while cur!=oldp:path.append(cur);cur=prev[cur]
    p=oldp
    for n in reversed(path):
     actions.append({'dx':n[0]-p[0],'dy':n[1]-p[1],'state':2 if n in walls or n in holes-filled else 0});p=n
    actions.append({'dx':d[0],'dy':d[1],'state':0});p=b;pushes+=1
  for b,goal in zip(((1,8),(6,13)),assignments):
   others=regular|(bridges-{b});chain=route_crate(p,b,goal,others,filled,True)
   if chain is None:break
   apply(chain,others);bridges.remove(b);filled.add(goal)
  else:
   checkpoint=(p,actions[:],pushes)
   for crates in itertools.permutations(regular):
    for goals in itertools.permutations(targets):
     p,actions,pushes=checkpoint[0],checkpoint[1][:],checkpoint[2];current=set(regular)
     for b,goal in zip(crates,goals):
      others=current-{b};chain=route_crate(p,b,goal,others,filled,False)
      if chain is None:break
      apply(chain,others);current.remove(b);current.add(goal)
     else:
      prev=walk(p,current);cur=(7,2);path=[]
      if cur not in prev:continue
      while cur!=p:path.append(cur);cur=prev[cur]
      for n in reversed(path):
       actions.append({'dx':n[0]-p[0],'dy':n[1]-p[1],'state':2 if n in walls else 0});p=n
      return actions,pushes
 raise RuntimeError('No solution found')
a,p=solve();Path(__file__).with_name('level1-solution.json').write_text(json.dumps({'actions':a}))
print(f'Solved: {p} pushes, {len(a)} moves; both holes filled, all three objectives covered, exit reached.')
