using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using appDiplo.Models;
using MoreLinq;
using System.Diagnostics;

namespace appDiplo
{
    /// <summary>
    /// Class that represents the position P of node N to insert to route M.
    /// </summary>
    public class AntNPM
    {
        public AntRoute M { get; set; }
        public POI N { get; set; }
        public int P { get; set; }
        public double Ratio { get; set; }
        public double RouteLength { get; set; }
        public int RouteInd { get; set; }
        
        /// <summary>
        /// Constructor of NPM class for ACO. Represents the information of feasible insertion of node <paramref name="n"/> at position <paramref name="p"/> in route <paramref name="m"/>.
        /// </summary>
        /// <param name="n">Node POI n representing a feasible POI inserted at position p to route m.</param>
        /// <param name="p">Integer value representing position of feasible insertion of node n to route m.</param>
        /// <param name="m">Route m to which the node can be inserted at positon p.</param>
        /// <param name="ratio">Computed double ratio of how appropriete is to insert node at given position.</param>
        /// <param name="routeLength">Length of the after insertion route.</param>
        /// <param name="routeInd">Index of the route in solution.</param>
        public AntNPM(POI n, int p, AntRoute m, double ratio, double routeLength, int routeInd)
        {
            N = n;
            P = p;
            M = m;
            Ratio = ratio;
            RouteLength = routeLength;
            RouteInd = routeInd;
        }
    }

    /// <summary>
    /// Class representing route of the day for ACO.
    /// </summary>
    public class AntRoute
    {
        public List<POI> RouteList { get; set; }
        public double Score { get; set; }
        public double Length { get; set; }
        private double originalLength;
        public int DayNum { get; set; }
        private POI startNode;
        private POI endNode;
        public AntRoute(POI startNode, POI endNode)
        {
            DayNum = 0;
            this.startNode = startNode;
            this.endNode = endNode;
            Length = 0;
            originalLength = 0;
            Score = 0;
        }

        /// <summary>
        /// Constructor of AntRoute class.
        /// </summary>
        /// <param name="dayNum">Integer value representing the day of week.</param>
        /// <param name="startNode">Node <seealso cref="POI"/> representing the start of the tour.</param>
        /// <param name="endNode">Node <seealso cref="POI"/> representing the send of the tour.</param>
        /// <param name="length">LTotal length of the route.</param>
        public AntRoute(int dayNum, POI startNode, POI endNode, double length)
        {
            Length = length;
            Score = 0;
            RouteList = new List<POI>();
            DayNum = dayNum;
            this.startNode = startNode;
            this.endNode = endNode;
            originalLength = length;
        }
        /// <summary>
        /// Method for adding node to the specified index of route.
        /// </summary>
        /// <param name="node">Node <seealso cref="POI"/> representing node to be inserted.</param>
        /// <param name="ind"></param>
        /// <param name="length">New length of the route after insertion.</param>
        public void AddNode(POI node, int ind, double length)
        {
            RouteList.Insert(ind, node);
            Score += node.Rating;
            Length = length;
        }
        /// <summary>
        /// Method that clears the route, sets score and length to zero.
        /// </summary>
        public void ClearRoute()
        {
            RouteList.Clear();
            Score = 0;
            Length = originalLength;
        }
        /// <summary>
        /// Method that returns either positive route length if the insertion is feasible or -1 else.
        /// </summary>
        /// <param name="node">Node <seealso cref="POI"/> representing the node to be inserted.</param>
        /// <param name="startNode">Node <seealso cref="POI"/> representing the start location of the route.</param>
        /// <param name="endNode">Node <seealso cref="POI"/> representing the end location of the route.</param>
        /// <param name="ind">Integer value representing the position of the insertion in the route to which the node is tested.</param>
        /// <returns>Returns either positive length of the route if the insertion is feasible or <c>-1</c> otherwise.</returns>
        public double TryInsert(POI node, POI startNode, POI endNode, int ind)
        {
            List<POI> new_route = new List<POI>(RouteList);
            new_route.Insert(ind, node);

            return ComputeRoute(new_route, startNode, endNode);
        }

        /// <summary>
        /// Mathod that returns either positive route length if the newly created <paramref name="route"/> is feasible.
        /// </summary>
        /// <param name="route">Route whose feasibility is tested.</param>
        /// <param name="startNode">Node <seealso cref="POI"/> representing the start location of the route.</param>
        /// <param name="endNode">Node <seealso cref="POI"/> representing the end location of the route.</param>
        /// <returns>Returns either positive nuber if <paramref name="route"/> is feasible or -1 otherwise.</returns>
        public double ComputeRoute(List<POI> route, POI startNode, POI endNode)
        {
            int act_ind;
            double route_length = startNode.Opening[DayNum].OpenHour;

            if (route.Count > 0)
            {
                if (!route[0].Opening.ContainsKey(DayNum))
                    return -1.0;
                act_ind = startNode.Neighbours.IndexOf(route[0]);
                route_length += startNode.Distance[act_ind];
                if (route_length < route[0].Opening[DayNum].OpenHour)
                    route_length = route[0].Opening[DayNum].OpenHour;
                else if (route_length > route[0].Opening[DayNum].CloseHour)
                    return -1.0;
                route_length += Convert.ToDouble(route[0].Duration);
                if (route_length > endNode.Opening[DayNum].CloseHour)
                    return -1.0;

                for (int i = 1; i < route.Count; i++)
                {
                    if (!route[i].Opening.ContainsKey(DayNum))
                        return -1.0;
                    act_ind = route[i - 1].Neighbours.IndexOf(route[i]);
                    route_length += route[i - 1].Distance[act_ind];
                    if (route_length < route[i].Opening[DayNum].OpenHour)
                        route_length = route[i].Opening[DayNum].OpenHour;
                    else if (route_length > route[i].Opening[DayNum].CloseHour)
                        return -1.0;
                    route_length += Convert.ToDouble(route[i].Duration);
                    if (route_length > endNode.Opening[DayNum].CloseHour)
                        return -1.0;
                }
                act_ind = route.Last().Neighbours.IndexOf(endNode);
                route_length += route.Last().Distance[act_ind];
                if (route_length > endNode.Opening[DayNum].CloseHour)
                    return -1.0;

                int length = 0;
                int dayLength = 0;
                int actInd;
                int hour, minute;

                actInd = startNode.Neighbours.IndexOf(route[0]);
                dayLength = startNode.Opening[DayNum].OpenHour;
                length += (int)startNode.Distance[actInd];
                dayLength += (int)startNode.Distance[actInd];
                route[0].Arrival = dayLength;
                hour = (int)Math.Floor((double)dayLength / 3600);
                minute = dayLength % 3600;
                minute = (int)Math.Floor((double)minute / 60);
                route[0].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute);
                length += (int)route[0].Duration;
                dayLength += (int)route[0].Duration;
                for (int j = 1; j < route.Count; j++)
                {
                    actInd = route[j - 1].Neighbours.IndexOf(route[j]);
                    length += (int)route[j - 1].Distance[actInd];
                    dayLength += (int)route[j - 1].Distance[actInd];
                    route[j].Arrival = dayLength;
                    hour = (int)Math.Floor((double)dayLength / 3600);
                    minute = length % 3600;
                    minute = (int)Math.Floor((double)minute / 60);
                    route[j].ArrivalString = string.Format("Arrival at {0}:{1}", hour, minute);
                    length += (int)route[j].Duration;
                    dayLength += (int)route[j].Duration;
                }
                actInd = endNode.Neighbours.IndexOf(route[route.Count - 1]);
                length += (int)endNode.Distance[actInd];
                dayLength += (int)endNode.Distance[actInd];
            }
            return route_length;
        }


    }
    /// <summary>
    /// Class representing ant for ACO.
    /// </summary>
    public class Ant
    {
        private double travelTime;
        private int dayInd;
        private POI startNode;
        private POI endNode;
        private List<int> days;
        private List<POI> N;
        private List<POI> NStar;
        private List<POI>[] relPOIDay;
        public double CumRating { get; set; }
        public AntRoute[] Routes;
        private double initPheromone;
        /// <summary>
        /// Constructor for the class representing ant.
        /// </summary>
        /// <param name="startNode">Node <seealso cref="POI"/> representing the start location of the route.</param>
        /// <param name="endNode">Node <seealso cref="POI"/> representing the end location of the route.</param>
        /// <param name="days"><seealso cref="List{int}"/> representing the sequence of days of the trip.</param>
        /// <param name="N"><seealso cref="List{POI}"/> of all the POIs taken into account when constructing the route.</param>
        /// <param name="relPOIDay"><seealso cref="Array {List{POI}}"/> representing nodes that are relevant for each day e. g. that are open at that day.</param>
        /// <param name="initPheromone">Parameter representing the initial value of pheromone level.</param>
        public Ant(POI startNode, POI endNode, List<int> days, List<POI> N, List<POI>[] relPOIDay, double initPheromone)
        {
            Routes = new AntRoute[days.Count];
            for (int i = 0; i < days.Count; i++)
            {
                Routes[i] = new AntRoute(days[i], startNode, endNode, startNode.Opening[days[i]].OpenHour);
            }
            travelTime = 0;
            this.startNode = startNode;
            this.endNode = endNode;
            this.days = days;
            dayInd = 0;
            travelTime = startNode.Opening[days[dayInd]].OpenHour;
            this.relPOIDay = relPOIDay;
            CumRating = 0;
            this.initPheromone = initPheromone;
            this.N = N;
            NStar = new List<POI>();
        }
        /// <summary>
        /// Method for local search of the solution.
        /// </summary>
        public void LocalSearch()
        {
            AntRoute[] swapped_routes2;
            AntRoute route;
            foreach (AntRoute actRoute in Routes)
            {
                Swap1(actRoute);
            }
            swapped_routes2 = Routes.OrderByDescending(o => o.Length).Take(2).ToArray();
            if(days.Count > 1)
                Swap2(swapped_routes2[0], swapped_routes2[1]);
            route = Routes.MaxBy(o => o.Length).First();
            Opt_2(route);
            if (days.Count > 1)
                Move();
            Insert();
            route = Routes.MinBy(s0 => s0.Length).First();
            Replace(route);
            foreach (AntRoute actRoute in Routes)
            {
                CumRating += actRoute.Score;
            }
        }
        /// <summary>
        /// Method for initializing the ant.
        /// </summary>
        /// <param name="N">List of POIs <seealso cref="List{POI}"/> that are updated each iteration.</param>
        public void Init(List<POI> N)
        {
            foreach (AntRoute route in Routes)
            {
                route.ClearRoute();
            }
            //Route.Clear();
            dayInd = 0;
            CumRating = 0;
            this.N = N.ToList();
            this.N.RemoveAt(0);
            this.N.RemoveAt(this.N.Count - 1);
        }
        /// <summary>
        /// Method for the choosing next POI to be visited according to probability.
        /// </summary>
        /// <param name="N">POIS that can be visited.</param>
        /// <param name="lastVisited">Last visited POI.</param>
        /// <param name="q">Parameter saying the chance of choosing the best POI to visit or randomly selected one.</param>
        /// <returns></returns>
        private POI StateTransition(List<POI> N, POI lastVisited, double q)
        {
            double bestRatio = 0;
            double actRatio = 0;
            double bestRouteLength = 0;
            double actRouteLength = 0;
            int actInd;
            Random random = new Random();
            POI selected = endNode;
            if (random.NextDouble() <= q)
            {
                for (int i = 0; i < N.Count; i++)
                {
                    actInd = lastVisited.Neighbours.IndexOf(N[i]);
                    actRatio = lastVisited.Pheromones[actInd] * Math.Pow(N[i].Rating,2) / lastVisited.Distance[actInd];
                    if (actRatio > bestRatio)
                    {
                        actRouteLength = TryInsert(lastVisited, N[i]);
                        if (actRouteLength > 0)
                        {
                            bestRatio = actRatio;
                            bestRouteLength = actRouteLength;
                            selected = N[i];
                        }
                    }
                }
                if (selected != endNode)
                    Routes[dayInd].Length = bestRouteLength;
                return selected;
            }
            else
            {
                selected = ChoosePOI(N, lastVisited);
                return selected;
            }
        }
        /// <summary>
        /// Method for choosing the next POI to visit according to Roulette Wheel Selection.
        /// </summary>
        /// <param name="N">List of nodes.</param>
        /// <param name="lastVisited">Last visited node.</param>
        /// <returns></returns>
        private POI ChoosePOI(List<POI> N, POI lastVisited)
        {
            double sum = 0;
            int actInd = 0;
            double cumRandProb = 0;
            Random random = new Random();
            double randProb = random.NextDouble();
            List<POI> NCopy = N.ToList();
            double routeLength;
            for (int i = 0; i < NCopy.Count; i++)
            {
                for (int j = 0; j < N.Count; j++)
                {
                    if (lastVisited != N[j])
                    {
                        actInd = lastVisited.Neighbours.IndexOf(N[j]);
                        sum += lastVisited.Pheromones[actInd] * Math.Pow(N[j].Rating, 2) / lastVisited.Distance[actInd];
                    }
                }
                for (int j = 0; j < N.Count; j++)
                {
                    if (lastVisited != N[j])
                    {
                        actInd = lastVisited.Neighbours.IndexOf(N[j]);
                        cumRandProb += (lastVisited.Pheromones[actInd] * (Math.Pow(N[j].Rating,2) / lastVisited.Distance[actInd])) / sum;
                        if (cumRandProb > randProb)
                            break;
                    }
                }
                if (lastVisited.Neighbours[actInd] == endNode)
                    return endNode;
                routeLength = TryInsert(lastVisited, lastVisited.Neighbours[actInd]);
                if (routeLength > 0)
                {
                    Routes[dayInd].Length = routeLength;
                    return lastVisited.Neighbours[actInd];
                }
                NCopy.Remove(lastVisited.Neighbours[actInd]);
            }
            return endNode;
        }
        /// <summary>
        /// Method for locally update of pheromones so the next ant chooses more likely different node to visit.
        /// </summary>
        /// <param name="lastVisited">Last visited node.</param>
        /// <param name="toVisit">Node to visit.</param>
        /// <param name="psi">Parameter for defining the new value of pheromone level of the edge.</param>
        private void LocalUpdate(POI lastVisited, POI toVisit, double psi)
        {
            int ind = lastVisited.Neighbours.IndexOf(toVisit);
            double deltaTau = initPheromone;
            lastVisited.Pheromones[ind] = lastVisited.Pheromones[ind] * (1 - psi) + psi * deltaTau;
        }

        /// <summary>
        /// Method that creates a feasible solution.
        /// </summary>
        /// <param name="psi">Parameter that affects the local update.</param>
        public void BuildSolution(double psi)
        {
            POI actPOI;
            List<POI> actN;
            while (dayInd < Routes.Length)
            {
                actN = relPOIDay[dayInd].ToList();
                for (int i = 0; i < Routes.Length; i++)
                {
                    for (int j = 0; j < Routes[i].RouteList.Count; j++)
                    {
                        if (actN.Contains(Routes[i].RouteList[j]))
                        {
                            actN.Remove(Routes[i].RouteList[j]);
                        }
                    }

                }

                while (actN.Count > 0)
                {
                    if (Routes[dayInd].RouteList.Count > 0)
                        actPOI = Routes[dayInd].RouteList.Last();
                    else
                        actPOI = startNode;
                    actPOI = StateTransition(actN, actPOI, psi);
                    if (actPOI == endNode)
                    {
                        LocalUpdate(Routes[dayInd].RouteList.Last(), endNode, psi);
                        break;
                    }
                    else
                    {
                        Routes[dayInd].RouteList.Add(actPOI);
                        if (Routes[dayInd].RouteList.Count > 1)
                            LocalUpdate(Routes[dayInd].RouteList[Routes[dayInd].RouteList.Count - 2], Routes[dayInd].RouteList.Last(), psi);
                        else
                            LocalUpdate(startNode, Routes[dayInd].RouteList.Last(), psi);
                        actN.Remove(actPOI);
                        N.Remove(actPOI);
                        NStar.Add(actPOI);
                        Routes[dayInd].Score += actPOI.Rating;
                        for (int i = 0; i < Routes.Length; i++)
                        {
                            if (Routes[i].ComputeRoute(Routes[i].RouteList, startNode, endNode) < 0)
                                Console.WriteLine("Huh");
                        }
                    }
                }
                dayInd++;
            }
        }
        /// <summary>
        /// Method that tries to insert given node after the given node.
        /// </summary>
        /// <param name="lastVisited">Last visited node.</param>
        /// <param name="toInsert">Node to be inserted.</param>
        /// <returns>Returns either positive lenght of the route or -1 if the insertion is infeasible.</returns>
        public double TryInsert(POI lastVisited, POI toInsert)
        {
            double newTravelTime = Routes[dayInd].Length;
            int actIndex = lastVisited.Neighbours.IndexOf(toInsert);
            if (newTravelTime + lastVisited.Distance[actIndex] < toInsert.Opening[days[dayInd]].OpenHour)
                newTravelTime = toInsert.Opening[days[dayInd]].OpenHour + (double)toInsert.Duration;
            else
                newTravelTime += lastVisited.Distance[actIndex] + (double)toInsert.Duration;
            actIndex = toInsert.Neighbours.IndexOf(endNode);
            if (newTravelTime <= toInsert.Opening[days[dayInd]].CloseHour)
            {
                if (newTravelTime + toInsert.Distance[actIndex] <= endNode.Opening[days[dayInd]].CloseHour)
                {
                    return newTravelTime;
                }
            }
            return -1.0;
        }
        /// <summary>
        /// Operator that swaps two nodes within one route.
        /// </summary>
        /// <param name="route">Route in which the nodes are tried to be swapped.</param>
        /// <returns>Returns either <c>true</c> if the operator was succesful or <c>false</c> otherwise.</returns>
        private bool Swap1(AntRoute route)
        {
            int act_ind;
            List<POI> new_route;
            POI swapped;
            double route_length;
            for (int i = 0; i < route.RouteList.Count; i++)
            {
                for (int j = i + 1; j < route.RouteList.Count; j++)
                {
                    route_length = 0;
                    new_route = new List<POI>(route.RouteList);
                    swapped = new_route[i];
                    new_route[i] = new_route[j];
                    new_route[j] = swapped;
                    route_length = route.ComputeRoute(new_route, startNode, endNode);
                    if (route_length < route.Length && route_length > 0)
                    {
                        route.RouteList = new_route;
                        route.Length = route_length;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Method for swapping two nodes between two routes.
        /// </summary>
        /// <param name="route1">First route of the nodes to be swapped.</param>
        /// <param name="route2">SEcond route of the nodes to be swapped.</param>
        /// <returns>Returns either <c>true</c> if the operator was succesful or <c>false</c> otherwise.</returns>
        private bool Swap2(AntRoute route1, AntRoute route2)
        {
            List<POI>[] new_routes = new List<POI>[2];
            POI swapped;
            double route_length1, route_length2;
            for (int i = 0; i < route1.RouteList.Count; i++)
            {
                for (int j = 0; j < route2.RouteList.Count; j++)
                {
                    route_length1 = 0;
                    route_length2 = 0;
                    new_routes[0] = new List<POI>(route1.RouteList);
                    new_routes[1] = new List<POI>(route2.RouteList);
                    swapped = route1.RouteList[i];
                    new_routes[0][i] = route2.RouteList[j];
                    new_routes[1][j] = swapped;
                    route_length1 = route1.ComputeRoute(new_routes[0], startNode, endNode);
                    if (route_length1 > 0 && route_length1 < route1.Length)
                    {
                        route_length2 = route2.ComputeRoute(new_routes[1], startNode, endNode);
                        if (route_length2 > 0 && route_length2 < route2.Length)
                        {
                            route1.Score = route1.Score - route1.RouteList[i].Rating + route2.RouteList[j].Rating;
                            route2.Score = route2.Score - route2.RouteList[j].Rating + route1.RouteList[i].Rating;
                            route1.RouteList = new_routes[0];
                            route1.Length = route_length1;
                            route2.RouteList = new_routes[1];
                            route2.Length = route_length2;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Operator that changes the order of nodes between two nodes.
        /// </summary>
        /// <param name="route">Route on which the operator is applied.</param>
        /// <returns>Returns either <c>true</c> if the operator was succesful or <c>false</c> otherwise.</returns>
        private bool Opt_2(AntRoute route)
        {
            double route_length;
            List<POI> new_route;
            for (int i = 0; i < route.RouteList.Count; i++)
            {
                for (int j = i + 1; j < route.RouteList.Count; j++)
                {
                    new_route = new List<POI>(route.RouteList);
                    new_route.Reverse(i, j - i + 1);
                    route_length = route.ComputeRoute(new_route, startNode, endNode);

                    if (route_length < route.Length && route_length > 0)
                    {
                        route.RouteList = new_route;
                        route.Length = route_length;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Method for creating <seealso cref="AntNPM"/> of feasible positions in routes of the nodes.
        /// </summary>
        /// <returns>Returns List of informations NPM.</returns>
        private List<AntNPM> UpdateF()
        {
            List<AntNPM> F = new List<AntNPM>();
            int f = 5;
            double new_distance, ratio;
            for (int i = 0; i < N.Count; i++) 
            {
                for (int j = 0; j < Routes.Length; j++) 
                {
                    for (int k = 0; k < Routes[j].RouteList.Count + 1; k++) 
                    {
                        new_distance = Routes[j].TryInsert(N[i], startNode, endNode, k);
                        if (new_distance >= 0) 
                        {
                            if (new_distance != Routes[j].Length)
                            {
                                ratio = Math.Pow(N[i].Rating, 2) / (Math.Abs(new_distance - Routes[j].Length)); 
                                F.Add(new AntNPM(N[i], k, Routes[j], ratio, new_distance, j));
                            }
                            else
                            {
                                ratio = Math.Pow(N[i].Rating, 2) / 0.01; 
                                F.Add(new AntNPM(N[i], k, Routes[j], ratio, new_distance, j));
                            }
                        }
                    }
                }
            }
            F = F.OrderByDescending(o => o.Ratio).Take(f).ToList();
            return F;
        }
        /// <summary>
        /// Mathod that tries to insert node <paramref name="node"/> only to one route <paramref name="M"/>
        /// </summary>
        /// <param name="node">Node to be inserted.</param>
        /// <param name="M">Route M to which the node shall be inserted.</param>
        /// <param name="ind">Integer value representing index of the route in the solution.</param>
        /// <returns>Returns list of information <seealso cref="NPM"/>.</returns>
        private List<AntNPM> UpdateF(POI node, AntRoute[] M, int ind)
        {
            List<AntNPM> F = new List<AntNPM>();
            double new_distance, ratio;
            int f = 5;

            for (int j = 0; j < M.Count(); j++) 
            {
                if (j != ind)
                {
                    for (int k = 1; k < M[j].RouteList.Count; k++) 
                    {
                        new_distance = M[j].TryInsert(node, startNode, endNode, k);
                        if (new_distance >= 0) 
                        {
                            if (new_distance != M[j].Length)
                            {
                                ratio = Math.Pow(node.Rating, 2) / (Math.Abs(new_distance - M[j].Length)); 
                                F.Add(new AntNPM(node, k, M[j], ratio, new_distance, j));
                            }
                            else
                            {
                                ratio = Math.Pow(node.Rating, 2) / 0.001;
                                F.Add(new AntNPM(node, k, M[j], ratio, new_distance, j));
                            }
                        }
                    }
                }
            }
            F = F.OrderByDescending(o => o.Ratio).Take(f).ToList();
            return F;
        }

        /// <summary>
        /// Operator that tries to move nodes from one route to another.
        /// </summary>
        /// <returns>Returns either <c>true</c> if the operator was succesful or <c>false</c> otherwise.</returns>
        private bool Move()
        {
            int i = 0;
            int j = 0;
            if (Routes[j].RouteList.Count > 0)
            {
                List<AntNPM> F = UpdateF(Routes[0].RouteList[i], Routes, j);
                AntNPM npm;
                while (F.Count == 0)
                {
                    i++;
                    if (i >= Routes[j].RouteList.Count)
                    {
                        j++;
                        if (j >= Routes.Count())
                            return false;
                        i = 0;
                    }
                    if (Routes[j].RouteList.Count > 0)
                        F = UpdateF(Routes[j].RouteList[i], Routes, j);
                }
                npm = Select(F);
                npm.M.AddNode(npm.N, npm.P, npm.RouteLength);
                Routes[j].RouteList.Remove(npm.N);
                Routes[j].Score -= npm.N.Rating;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Operator that tries to insert the yet unscheduled nodes to the solution.
        /// </summary>
        /// <returns>Returns either <c>true</c> if the operator was succesful or <c>false</c> otherwise.</returns>
        private bool Insert()
        {
            AntNPM npm;
            List<AntNPM> F;
            if (N.Count > 0)
            {
                F = UpdateF();
                while (F.Count > 0)
                {
                    npm = Select(F);
                    npm.M.AddNode(npm.N, npm.P, npm.RouteLength);
                    N.Remove(npm.N);
                    NStar.Add(npm.N);
                    F = UpdateF();
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Selects the node n to be inserted at position p of route m.
        /// </summary>
        /// <param name="F"></param>
        /// <returns>Either chosen <seealso cref="NPM"/> or first one.</returns>
        private AntNPM Select(List<AntNPM> F)
        {
            double sumRatio = 0;
            Random random = new Random();
            double u = random.NextDouble();
            double accumProb = 0;
            foreach (AntNPM npm in F)
            {
                sumRatio += npm.Ratio;
            }
            foreach (AntNPM npm in F)
            {
                accumProb += npm.Ratio / sumRatio;
                if (u < accumProb)
                {
                    return npm;
                }
            }
            return F[0];
        }
        /// <summary>
        /// Operator that replaces the node in the solution with unscheduled node.
        /// </summary>
        /// <param name="route"></param>
        /// <returns>Returns either <c>true</c> if the operator was succesful or <c>false</c> otherwise.</returns>
        private bool Replace(AntRoute route)
        {
            double route_length, score;
            List<POI> new_route;
            List<POI> N_temp = new List<POI>(N);
            List<POI> N_new = new List<POI>();
            POI? replaced;
            int act_ind;
            bool not_bad_route;
            bool changed = false;
            for (int i = 0; i < N.Count; i++)
            {
                replaced = N_temp.MaxBy(o => o.Rating).First();
                if (replaced.Opening.ContainsKey(route.DayNum))
                {
                    N_new.Add(replaced);
                    for (int j = 0; j < route.RouteList.Count; j++)
                    {
                        route_length = startNode.Opening[route.DayNum].OpenHour;

                        not_bad_route = true;
                        new_route = new List<POI>(route.RouteList);
                        new_route[j] = replaced;

                        new_route[j] = replaced;
                        score = new_route[0].Rating;
                        act_ind = startNode.Neighbours.IndexOf(new_route[0]);
                        route_length += startNode.Distance[act_ind] + (double)new_route[0].Duration;

                        score = new_route[0].Rating;
                        for (int k = 1; k < route.RouteList.Count; k++)
                        {
                            if (new_route[k].Opening.ContainsKey(route.DayNum))
                            {
                                act_ind = new_route[k - 1].Neighbours.IndexOf(new_route[k]);
                                route_length += new_route[k - 1].Distance[act_ind];
                                score += new_route[k].Rating;
                                if (route_length < new_route[k].Opening[route.DayNum].OpenHour)
                                    route_length = new_route[k].Opening[route.DayNum].OpenHour;
                                else if (route_length > new_route[k].Opening[route.DayNum].CloseHour)
                                {
                                    not_bad_route = false;
                                    break;
                                }
                                route_length += Convert.ToDouble(new_route[k].Duration); 
                                if (route_length > endNode.Opening[route.DayNum].CloseHour)
                                {
                                    not_bad_route = false;
                                    break;
                                }

                            }
                        }
                        act_ind = new_route.Last().Neighbours.IndexOf(endNode);
                        route_length += new_route.Last().Distance[act_ind];
                        if (route_length > endNode.Opening[route.DayNum].CloseHour)
                            not_bad_route = false;
                        if (score > route.Score && not_bad_route)
                        {
                            N_new.RemoveAt(N_new.Count - 1);
                            N_new.Add(route.RouteList[j]);
                            route.RouteList[j] = replaced;
                            route.Score = score;
                            changed = true;
                            break;
                        }
                    }
                    N_temp.Remove(replaced);
                }
            }
            N = N_new;
            return changed;
        }


    }
    /// <summary>
    /// Class representing ACS algorithm.
    /// </summary>
    public class ACO
    {
        public List<Ant> Ants { get; set; }
        private int antNum;
        public POI startNode;
        public POI endNode;
        private List<int> days;
        private List<POI> N;
        public Ant AllTimeBestAnt;
        private double rho;
        private double psi;
        private List<POI>[]? relPOIDay;

        public double BestScore { get; set; }
        public List<AntRoute> BestRoute { get; set; }
        public ACO(int antN, List<POI> pois, POI startNode, POI endNode, List<int> days)
        {
            Ants = new List<Ant>(antN);
            antNum = antN;
            this.startNode = startNode;
            this.endNode = endNode;
            this.days = days;
            N = pois.ToList();
            N.RemoveAt(0);
            N.RemoveAt(N.Count - 1);
            CreateDaysPOIs(N);
            rho = 0;
            BestScore = 0;
            BestRoute = new List<AntRoute>();
            for (int i = 0; i < days.Count; i++)
                BestRoute.Add(new AntRoute(startNode, endNode));
        }

        /// <summary>
        /// Method that creates array of lists of POIs relevant to days of the week of the trip.
        /// </summary>
        /// <param name="allPOIs">All POIs taking into account.</param>
        private void CreateDaysPOIs(List<POI> allPOIs)
        {
            relPOIDay = new List<POI>[days.Count];
            for (int i = 0; i < days.Count; i++)
            {
                relPOIDay[i] = new List<POI>();
            }
            foreach (POI poi in allPOIs)
            {
                for (int i = 0; i < days.Count; i++)
                {
                    if (poi.Opening.ContainsKey(days[i]))
                        relPOIDay[i].Add(poi);
                }
            }
        }

        /// <summary>
        /// Method that initializes pheromone levels.
        /// </summary>
        /// <param name="initPheromone">Initial value of pheromones.</param>
        private void PheromonesInit(double initPheromone)
        {
            startNode.Pheromones = new List<double>();
            for (int i = 0; i < startNode.Neighbours.Count; i++)
            {
                startNode.Pheromones.Add(initPheromone);
            }
            foreach (POI poi in N)
            {
                poi.Pheromones = new List<double>();
                for (int i = 0; i < poi.Neighbours.Count; i++)
                {
                    poi.Pheromones.Add(initPheromone);
                }
            }
        }
        /// <summary>
        /// Method that inicializes all ants.
        /// </summary>
        /// <param name="pois">List of all POIs.</param>
        /// <param name="initPheromone">Initial value of pheromoene.</param>
        private void AntsInit(List<POI> pois, double initPheromone)
        {
            for (int i = 0; i < antNum; i++)
            {
                Ants.Add(new Ant(startNode, endNode, days, pois, relPOIDay, initPheromone));
            }

        }

        /// <summary>
        /// Method that globally updates pheromone level. Only best ant updates his pheromone trail. In addition on each edge of the graph the pheromones evaporates.
        /// </summary>
        /// <param name="bestAnt"></param>
        private void GlobalUpdate(Ant bestAnt)
        {
            int ind;
            POI actPOI;
            for (int i = 0; i < startNode.Pheromones.Count; i++)
            {
                startNode.Pheromones[i] = startNode.Pheromones[i] * (1 - rho);
            }
            foreach (POI poi in N)
            {
                for (int i = 0; i < poi.Pheromones.Count; i++)
                {
                    poi.Pheromones[i] = poi.Pheromones[i] * (1 - rho);
                }
            }

            for (int i = 0; i < bestAnt.Routes.Length; i++)
            {
                if (bestAnt.Routes[i].RouteList.Count > 0)
                {
                    ind = startNode.Neighbours.IndexOf(bestAnt.Routes[i].RouteList[0]);
                    startNode.Pheromones[ind] += bestAnt.CumRating;
                    for (int j = 0; j < bestAnt.Routes[i].RouteList.Count - 1; j++)
                    {
                        actPOI = bestAnt.Routes[i].RouteList[j];
                        ind = actPOI.Neighbours.IndexOf(bestAnt.Routes[i].RouteList[j + 1]);
                        if (ind >= 0)
                            actPOI.Pheromones[ind] += bestAnt.CumRating;
                    }
                    ind = bestAnt.Routes[i].RouteList.Last().Neighbours.IndexOf(endNode);
                    bestAnt.Routes[i].RouteList.Last().Pheromones[ind] += bestAnt.CumRating;
                }
            }
        }

        /// <summary>
        /// Method that starts computing of the ACS algorithm.
        /// </summary>
        /// <param name="timeLimit">Time budget of the equotation.</param>
        /// <param name="initPheromone">Initial value of the pheromone.</param>
        /// <param name="rho">Parameter affecting global update.</param>
        /// <param name="psi">Parameter affecting local update.</param>
        /// <param name="q0">Parameter saying the probability in state transition.</param>
        public void StartACO(int timeLimit, double initPheromone, double rho, double psi, double q0)
        {
            this.rho = rho;
            this.psi = psi;
            int j;
            int i = 0;
            AntsInit(N, initPheromone);
            PheromonesInit(initPheromone);
            Ant bestAnt = Ants[0];
            Ant bestAllTimeAnt = Ants[0];
            string log = "";
            double actBestScore = 0;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            var elapsed = watch.Elapsed;
            double elapsedSeconds = elapsed.TotalSeconds;

            while ((double)timeLimit > elapsedSeconds)
            {
                i++;
                actBestScore = 0;
                j = 0;
                foreach (Ant ant in Ants)
                {
                    j++;
                    ant.Init(N);
                    ant.BuildSolution(psi);
                    ant.LocalSearch();
                    if (ant.CumRating > actBestScore)
                    {
                        bestAnt = ant;
                        actBestScore = ant.CumRating;
                    }
                }
                GlobalUpdate(bestAnt);
                if (bestAnt.CumRating > BestScore)
                {
                    BestScore = bestAnt.CumRating;
                    for (int k = 0; k < days.Count; k++)
                    {
                        BestRoute[k].RouteList = bestAnt.Routes[k].RouteList.ToList();
                        BestRoute[k].DayNum = bestAnt.Routes[k].DayNum;
                        BestRoute[k].Score = bestAnt.Routes[k].Score;
                        BestRoute[k].Length = bestAnt.Routes[k].Length;
                    }
                    Console.WriteLine(String.Format("Generation{0}: New Best {1}", i, bestAnt.CumRating));
                }
                elapsed = watch.Elapsed;
                elapsedSeconds = elapsed.TotalSeconds;
            }
        }
    }
}
