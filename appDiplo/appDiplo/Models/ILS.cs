using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MoreLinq;

namespace appDiplo.Models
{
    /// <summary>
    /// Class representing route of the trip for one day.
    /// </summary>
    class Route
    {
        public List<POI> RouteList { get; set; }
        public double Length { get; set; }
        public double Score { get; set; }
        public int DayNum { get; set; }
        /// <summary>
        /// Constructor of NPM class for ILS, SAILS and SAILSACS Represents the information of feasible insertion of node <paramref name="n"/> at position <paramref name="p"/> in route <paramref name="m"/>.
        /// </summary>
        /// <param name="n">Node POI n representing a feasible POI inserted at position p to route m.</param>
        /// <param name="p">Integer value representing position of feasible insertion of node n to route m.</param>
        /// <param name="m">Route m to which the node can be inserted at positon p.</param>
        /// <param name="ratio">Computed double ratio of how appropriete is to insert node at given position.</param>
        /// <param name="routeLength">Length of the after insertion route.</param>
        public Route(List<POI> poi_list, double length, double score, int dayNum)
        {
            RouteList = poi_list;
            Length = length;
            Score = score;
            DayNum = dayNum;
        }
        /// <summary>
        /// Constructor with only given day number.
        /// </summary>
        /// <param name="dayNum">Number of the day in the week.</param>
        public Route(int dayNum)
        {
            Length = 0;
            Score = 0;
            RouteList = new List<POI>();
            DayNum = dayNum;
        }
        /// <summary>
        /// Inserts node at given position and updates lenght and score of the route acoordinely.
        /// </summary>
        /// <param name="node">Node to insert.</param>
        /// <param name="ind">Index at which to insert node.</param>
        /// <param name="length">Length of the route after insertion.</param>
        public void AddNode(POI node, int ind, double length)
        {
            RouteList.Insert(ind, node);
            Score += node.Rating;
            Length = length;
        }
        /// <summary>
        /// Method that computes route lengthe.
        /// </summary>
        /// <param name="route">Route to which to compute routelength.</param>
        /// <param name="startNode">Starting location.</param>
        /// <param name="endNode">End location.</param>
        /// <param name="dayNum">Day number.</param>
        /// <returns>Either -1 is solution is infeasible or positive length.</returns>
        public double ComputeRoute(List<POI> route, POI startNode, POI endNode, int dayNum)
        {
            int act_ind;
            double route_length = startNode.Opening[dayNum].OpenHour;

            if (route.Count > 0)
            {
                if (!route[0].Opening.ContainsKey(dayNum))
                    return -1.0;
                act_ind = startNode.Neighbours.IndexOf(route[0]);
                route_length += startNode.Distance[act_ind];
                if (route_length < route[0].Opening[dayNum].OpenHour)
                    route_length = route[0].Opening[dayNum].OpenHour;
                else if (route_length > route[0].Opening[dayNum].CloseHour)
                {
                    return -1.0;
                }

                route_length += Convert.ToDouble(route[0].Duration); // change Duration to double...
                if (route_length > endNode.Opening[dayNum].CloseHour)
                {
                    return -1.0;
                }

                for (int i = 1; i < route.Count; i++)
                {
                    if (!route[i].Opening.ContainsKey(dayNum))
                        return -1.0;
                    act_ind = route[i - 1].Neighbours.IndexOf(route[i]);
                    route_length += route[i - 1].Distance[act_ind];
                    if (route_length < route[i].Opening[dayNum].OpenHour)
                        route_length = route[i].Opening[dayNum].OpenHour;
                    else if (route_length > route[i].Opening[dayNum].CloseHour)
                    {
                        return -1.0;
                    }
                    route_length += Convert.ToDouble(route[i].Duration); // change Duration to double...
                    if (route_length > endNode.Opening[dayNum].CloseHour)
                    {
                        return -1.0;
                    }
                }
                act_ind = route.Last().Neighbours.IndexOf(endNode);
                route_length += route.Last().Distance[act_ind];
                if (route_length > endNode.Opening[dayNum].CloseHour)
                {
                    return -1.0;
                }
            }
            return route_length;
        }
        /// <summary>
        /// Method that computes route in this actual route.
        /// </summary>
        /// <param name="startNode">Starting location of the route.</param>
        /// <param name="endNode">End location of the route.</param>
        /// <param name="dayNum">Number of the day.</param>
        /// <returns>Either -1 is solution is infeasible or positive length.</returns>
        public double ComputeRoute(POI startNode, POI endNode, int dayNum)
        {
            double route_length = startNode.Opening[dayNum].OpenHour;
            int act_ind;
            if (RouteList.Count > 0)
            {
                if (!RouteList[0].Opening.ContainsKey(dayNum))
                    return -1.0;
                act_ind = startNode.Neighbours.IndexOf(RouteList[0]);
                route_length += startNode.Distance[act_ind];
                if (route_length < RouteList[0].Opening[dayNum].OpenHour)
                    route_length = RouteList[0].Opening[dayNum].OpenHour;
                else if (route_length > RouteList[0].Opening[dayNum].CloseHour)
                    return -1.0;
                route_length += Convert.ToDouble(RouteList[0].Duration); // change Duration to double...
                if (route_length > endNode.Opening[dayNum].CloseHour)
                    return -1.0;

                for (int i = 1; i < RouteList.Count; i++)
                {
                    if (!RouteList[i].Opening.ContainsKey(dayNum))
                        return -1.0;
                    act_ind = RouteList[i - 1].Neighbours.IndexOf(RouteList[i]);
                    route_length += RouteList[i - 1].Distance[act_ind];
                    if (route_length < RouteList[i].Opening[dayNum].OpenHour)
                        route_length = RouteList[i].Opening[dayNum].OpenHour;
                    else if (route_length > RouteList[i].Opening[dayNum].CloseHour)
                        return -1.0;
                    route_length += Convert.ToDouble(RouteList[i].Duration); // change Duration to double...
                    if (route_length > endNode.Opening[dayNum].CloseHour)
                        return -1.0;
                }
                act_ind = RouteList.Last().Neighbours.IndexOf(endNode);
                route_length += RouteList.Last().Distance[act_ind];
                if (route_length > endNode.Opening[dayNum].CloseHour)
                    return -1.0;
            }

            return route_length;
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
            // asi joooo
            // tries to insert given node and returns routeLength and score of the route
            List<POI> new_route = new List<POI>(RouteList);
            new_route.Insert(ind, node);

            return ComputeRoute(new_route, startNode, endNode, DayNum);
        }

        /// <summary>
        /// Method that creates copy of this route.
        /// </summary>
        /// <returns>Copy of the route.</returns>
        public Route CopyRoute()
        {
            return new Route(new List<POI>(RouteList), Length, Score, DayNum);
        }
    }
    /// <summary>
    /// Class that represents the position P of node N to insert to route M.
    /// </summary>
    class NPM
    {
        public Route M { get; set; }
        public POI N { get; set; }
        public int P { get; set; }
        public double Ratio { get; set; }
        public double RouteLength { get; set; }
        public int RouteInd { get; set; }
        /// <summary>
        /// Constructor of NPM class. Represents the information of feasible insertion of node <paramref name="n"/> at position <paramref name="p"/> in route <paramref name="m"/>.
        /// </summary>
        /// <param name="n">Node POI n representing a feasible POI inserted at position p to route m.</param>
        /// <param name="p">Integer value representing position of feasible insertion of node n to route m.</param>
        /// <param name="m">Route m to which the node can be inserted at positon p.</param>
        /// <param name="ratio">Computed double ratio of how appropriete is to insert node at given position.</param>
        /// <param name="routeLength">Length of the after insertion route.</param>
        /// <param name="routeInd">Index of the route in solution.</param>
        public NPM(POI n, int p, Route m, double ratio, double routeLength, int routeInd)
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
    /// Base class for ILS algorithms.
    /// </summary>
    internal class ILSBase
    {
        protected Route[] S0 { get; set; }
        protected double S0_score;
        protected Graph G { get; set; }
        protected int exchangeInd;
        protected List<POI> N;
        protected List<POI> N_star;
        protected int cons;
        protected int post;
        protected int threshold1;
        protected int threshold2;
        protected int threshold3;
        protected int f;
        protected int iter;
        public POI startNode;
        public POI endNode;
        public Route[] S0_star { get; set; }
        public double S0_star_score { get; protected set; }
        protected List<int> days;
        protected int[] check;

        /// <summary>
        /// Operator that swaps two nodes within one route.
        /// </summary>
        /// <param name="route">Route in which the nodes are tried to be swapped.</param>
        /// <returns>Returns either <c>true</c> if the operator was succesful or <c>false</c> otherwise.</returns>
        private bool Swap1(Route route)
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
                    swapped = route.RouteList[i];
                    if (swapped.Opening.ContainsKey(route.DayNum))
                    {
                        new_route = new List<POI>(route.RouteList);
                        new_route[i] = new_route[j];
                        new_route[j] = swapped;
                        route_length = route.ComputeRoute(new_route, startNode, endNode, route.DayNum);
                        if (route_length < route.Length && route_length > 0)
                        {
                            route.RouteList = new_route;
                            route.Length = route_length;
                            return true;
                        }
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
        private bool Swap2(Route route1, Route route2)
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
                    if (route1.RouteList[i].Opening.ContainsKey(route2.DayNum) && route2.RouteList[j].Opening.ContainsKey(route1.DayNum))
                    {
                        new_routes[0] = new List<POI>(route1.RouteList);
                        new_routes[1] = new List<POI>(route2.RouteList);
                        swapped = route1.RouteList[i];
                        new_routes[0][i] = route2.RouteList[j];
                        new_routes[1][j] = swapped;
                        route_length1 = route1.ComputeRoute(new_routes[0], startNode, endNode, route1.DayNum);
                        if (route_length1 > 0 && route_length1 < route1.Length)
                        {
                            route_length2 = route2.ComputeRoute(new_routes[1], startNode, endNode, route2.DayNum);
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
            }
            return false;
        }
        /// <summary>
        /// Operator that changes the order of nodes between two nodes.
        /// </summary>
        /// <param name="route">Route on which the operator is applied.</param>
        private bool Opt_2(Route route)
        {
            double route_length;
            List<POI> new_route;
            for (int i = 0; i < route.RouteList.Count; i++)
            {
                for (int j = i + 1; j < route.RouteList.Count; j++)
                {
                    new_route = new List<POI>(route.RouteList);
                    new_route.Reverse(i, j - i + 1);
                    route_length = route.ComputeRoute(new_route, startNode, endNode, route.DayNum);

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
        /// Operator that tries to move nodes from one route to another.
        /// </summary>
        /// <returns>Returns either <c>true</c> if the operator was succesful or <c>false</c> otherwise.</returns>
        private bool Move()
        {
            int i = 0;
            int j = 0;
            if (S0[j].RouteList.Count > 0)
            {
                List<NPM> F = UpdateF(S0[0].RouteList[i], S0, j);
                NPM npm;
                while (F.Count == 0)
                {
                    i++;
                    if (i >= S0[j].RouteList.Count)
                    {
                        j++;
                        if (j >= S0.Count())
                            return false;
                        i = 0;
                    }
                    if (S0[j].RouteList.Count > 0)
                        F = UpdateF(S0[j].RouteList[i], S0, j);
                }
                npm = Select(F);
                npm.M.AddNode(npm.N, npm.P, npm.RouteLength);
                S0[j].RouteList.Remove(npm.N);
                S0[j].ComputeRoute(startNode, endNode, S0[j].DayNum);
                S0[j].Score -= npm.N.Rating;
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
            NPM npm;
            List<NPM> F;
            if (N.Count > 0)
            {
                F = UpdateF();
                while (F.Count > 0)
                {
                    npm = Select(F);
                    npm.M.AddNode(npm.N, npm.P, npm.RouteLength);
                    N.Remove(npm.N);
                    N_star.Add(npm.N);
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
        private bool Replace(Route route)
        {
            double route_length, score;
            List<POI> new_route;
            List<POI> N_temp = new List<POI>(N);
            List<POI> N_new = new List<POI>();
            POI? replaced;
            int act_ind;
            bool not_bad_route;
            bool changed = false;
            S0_score = ComputeScoreRoute(S0);
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
                        score = new_route[0].Rating;
                        act_ind = startNode.Neighbours.IndexOf(new_route[0]);
                        route_length += startNode.Distance[act_ind] + (double)new_route[0].Duration;

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
                            not_bad_route=false;
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

        /// <summary>
        /// Method that applies perturbation to the solution if there was no improvement for given iteration.
        /// </summary>
        /// <param name="noImpr">Parameter affecting the number of iteration without improvement.</param>
        protected void Perturbation(int noImpr)
        {
            Route? minRoute, maxRoute;
            Route[] minRoutes = new Route[S0.Length];
            int j = 0;
            if (noImpr > threshold2 && (noImpr + 1) % threshold3 == 0)
            {
                if (days.Count > 1)
                {
                    ExchangePath(S0, exchangeInd);
                    if (exchangeInd - 1 == 0)
                        exchangeInd = S0.Count() - 1;
                }
            }
            else
            {
                S0.CopyTo(minRoutes, 0);
                minRoutes = minRoutes.OrderBy(o => o.RouteList.Count).ToArray();
                minRoute = minRoutes[j];
                maxRoute = S0.MaxBy(o => o.RouteList.Count).First();
                if (post > minRoute.RouteList.Count)
                {
                    post -= (minRoute.RouteList.Count);
                }
                if (cons > maxRoute.RouteList.Count)
                    cons = 1;
                Shake(S0);
                post += cons;
            }
        }

        /// <summary>
        /// Operator that changes position of paths in the solution.
        /// </summary>
        /// <param name="S0">Current soluteion.</param>
        /// <param name="ind">Index of path to be put up.</param>
        private void ExchangePath(Route[] S0, int ind)
        {
            Route exchanged = S0[ind];
            S0[ind] = S0[ind - 1];
            S0[ind - 1] = exchanged;
        }
        /// <summary>
        /// Oparator that removes POIs from solution.
        /// </summary>
        /// <param name="S0">Actual solution.</param>
        private void Shake(Route[] S0)
        {
            int i, ind;
            POI routeRemoved;
            for (int j = 0; j < S0.Count(); j++)
            {
                i = 0;
                ind = post;
                while (i < cons)
                {
                    if (ind >= S0[j].RouteList.Count)
                    {
                        ind = 0;
                    }
                    if (S0[j].RouteList.Count < 1)
                        break;
                    routeRemoved = S0[j].RouteList[ind];
                    S0[j].RouteList.Remove(routeRemoved);
                    S0[j].Score -= routeRemoved.Rating;
                    N.Add(routeRemoved);
                    N_star.Remove(routeRemoved);
                    i++;
                    ind++;
                }
                S0[j].ComputeRoute(S0[j].RouteList, startNode, endNode, S0[j].DayNum);
            }
        }
        /// <summary>
        /// Local search for only one day trip.
        /// </summary>
        /// <returns>Current solution.</returns>
        protected Route[] LocalSearchOPTW()
        {
            Route? route = S0.MaxBy(o => o.Length).First();
            Route[] swapped_routes2;
            //Console.WriteLine();
            if (Swap1(route))
                check[0]++;
            swapped_routes2 = S0.OrderByDescending(o => o.Length).Take(2).ToArray();
            route = S0.MaxBy(o => o.Length).First();
            if (Opt_2(route))
                check[2]++;
            if (Insert())
                check[4]++;
            route = S0.MinBy(s0 => s0.Length).First();
            if (Replace(route))
                check[5]++;
            return S0;
        }

        /// <summary>
        /// Local search for multiple day trips.
        /// </summary>
        /// <returns>Current solution.</returns>
        protected Route[] LocalSearch()
        {
            Route? route = S0.MaxBy(o => o.Length).First();
            Route[] swapped_routes2;
            //Console.WriteLine();
            if (Swap1(route))
                check[0]++;
            swapped_routes2 = S0.OrderByDescending(o => o.Length).Take(2).ToArray();
            if (Swap2(swapped_routes2[0], swapped_routes2[1]))
                check[1]++;
            route = S0.MaxBy(o => o.Length).First();
            if (Opt_2(route))
                check[2]++;
            if (Move())
                check[3]++;
            if (Insert())
                check[4]++;
            route = S0.MinBy(s0 => s0.Length).First();
            if (Replace(route))
                check[5]++;
            return S0;
        }

        /// <summary>
        /// Initial construction of solution.
        /// </summary>
        /// <returns>Current solution.</returns>
        protected Route[] StartConstruction()
        {
            NPM npm;
            List<NPM> F = UpdateF();
            while (F.Count > 0)
            {
                npm = Select(F);
                npm.M.AddNode(npm.N, npm.P, npm.RouteLength);
                N.Remove(npm.N);
                N_star.Add(npm.N);
                F = UpdateF();
            }
            return S0;
        }
        /// <summary>
        /// Method for creating <seealso cref="NPM"./>
        /// </summary>
        /// <returns>List of NPM.</returns>
        private List<NPM> UpdateF() // should check for days
        {
            List<NPM> F = new List<NPM>();
            //int f = 10;
            double new_distance, ratio;
            for (int i = 0; i < N.Count; i++) // foreach N in N' (pro kazdy unscheduled node)
            {
                for (int j = 0; j < days.Count; j++) // foreach m in M (parametr mu = pocet cest)
                {
                    for (int k = 0; k < S0[j].RouteList.Count + 1; k++) // foreach p(m) in P(m) (pro kazdou pozici, kam je mozne vlozit uzel) napr uzel se tremi scheduled nodes -> pozice 0, 1, 2, 3
                    {
                        if (N[i].Opening.ContainsKey(S0[j].DayNum))
                        {
                            new_distance = S0[j].TryInsert(N[i], startNode, endNode, k);
                            if (new_distance >= 0) // if insertion creates feasible route (returns positive route_length instead of -1)
                            {
                                if (new_distance != S0[j].Length)
                                {
                                    ratio = Math.Pow(N[i].Rating, 2) / (Math.Abs(new_distance - S0[j].Length)); // computes ratio (dividing by zero????)
                                    F.Add(new NPM(N[i], k, S0[j], ratio, new_distance, j));
                                }
                                else
                                {
                                    ratio = Math.Pow(N[i].Rating, 2) / 0.01; // computes ratio (dividing by zero????)
                                    F.Add(new NPM(N[i], k, S0[j], ratio, new_distance, j));
                                }
                            }
                        }
                    }
                }
            }
            F = F.OrderByDescending(o => o.Ratio).Take(f).ToList();
            return F;
        }
        /// <summary>
        /// Creates <seealso cref="NPM"/> for the given node <paramref name="node"/> and route <paramref name="M"/>
        /// </summary>
        /// <param name="node">Node to be inserted.</param>
        /// <param name="M">Route to wich the node should be inserted.</param>
        /// <param name="ind">Integer value of index of route in the solution.</param>
        /// <returns>List of NPM.</returns>
        private List<NPM> UpdateF(POI node, Route[] M, int ind)
        {
            List<NPM> F = new List<NPM>();
            double new_distance, ratio;

            for (int j = 0; j < M.Count(); j++)
            {
                if (j != ind)
                {
                    for (int k = 1; k < M[j].RouteList.Count; k++)
                    {
                        if (node.Opening.ContainsKey(M[j].DayNum))
                        {
                            new_distance = M[j].TryInsert(node, startNode, endNode, k);
                            if (new_distance >= 0)
                            {
                                if (new_distance != M[j].Length)
                                {
                                    ratio = Math.Pow(node.Rating, 2) / (Math.Abs(new_distance - M[j].Length));
                                    F.Add(new NPM(node, k, M[j], ratio, new_distance, j));
                                }
                                else
                                {
                                    ratio = Math.Pow(node.Rating, 2) / 0.001;
                                    F.Add(new NPM(node, k, M[j], ratio, new_distance, j));
                                }
                            }
                        }
                    }
                }
            }
            F = F.OrderByDescending(o => o.Ratio).Take(f).ToList();
            return F;
        }
        /// <summary>
        /// Computes score of the route.
        /// </summary>
        /// <param name="route"></param>
        /// <returns>Score of the route.</returns>
        protected double ComputeScoreRoute(Route[] route)
        {
            double score = 0.0;
            foreach (Route r in route)
            {
                score += r.Score;
            }
            return score;
        }
        /// <summary>
        /// Select NPM from list of NPMs.
        /// </summary>
        /// <param name="F">List of NPM.</param>
        /// <returns>Selected NPM.</returns>
        private NPM? Select(List<NPM> F)
        {
            double sumRatio = 0;
            Random random = new Random();
            double u = random.NextDouble();
            double accumProb = 0;
            foreach (NPM npm in F)
            {
                sumRatio += npm.Ratio;
            }
            foreach (NPM npm in F)
            {
                accumProb += npm.Ratio / sumRatio;
                if (u < accumProb)
                {
                    return npm;
                }
            }
            return F[0];
        }
    }

    /// <summary>
    /// Class representing ILS algorithm.
    /// </summary>
    internal class ILS : ILSBase
    {
        /// <summary>
        /// Constructor of ILS class.
        /// </summary>
        public ILS() { }
        /// <summary>
        /// Constructor of the ILS class.
        /// </summary>
        /// <param name="g">Graph of the problem.</param>
        /// <param name="days">List of days of the trip.</param>
        /// <param name="threshold1">Parameter affecting if the algorithm should search localy around best so far solution.</param>
        /// <param name="threshold2">Parameter affecting perturbation.</param>
        /// <param name="threshold3">Parameter affecting perturbation.</param>
        /// <param name="f">Number of NPMs to take for another choice.</param>
        /// <param name="startNode">Start node of the trips.</param>
        /// <param name="endNode">End node of the trips.</param>
        public ILS(Graph g, List<int> days, int threshold1, int threshold2, int threshold3, int f, POI startNode, POI endNode)
        {
            G = g;
            exchangeInd = days.Count - 1;
            S0 = new Route[days.Count];
            for (int i = 0; i < days.Count; i++)
            {
                S0[i] = new Route(days[i]);
            }
            N = g.Nodes.ToList();
            N.Remove(N[0]);
            N.Remove(N[N.Count - 1]);
            N_star = new List<POI>();
            cons = 1;
            post = 0;
            this.threshold1 = threshold1;
            this.threshold2 = threshold2;
            this.threshold3 = threshold3;
            this.f = f;
            S0_score = 0;
            S0_star_score = 0;
            S0_star = new Route[days.Count];
            iter = 0;
            this.startNode = startNode;
            this.endNode = endNode;
            this.days = days;
            check = new int[6] { 0, 0, 0, 0, 0, 0 };
        }
        /// <summary>
        /// Method for starting algoirthm
        /// </summary>
        /// <param name="timeLimit">Time budget for equation.</param>
        /// <returns>Best found solution.</returns>
        public async Task StartILS(int timeLimit)
        {
            int noImprovment = 0;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var elapsed = watch.Elapsed;
            double elapsedSeconds = elapsed.TotalSeconds;
            S0 = StartConstruction();
            if (days.Count > 1)
                S0 = LocalSearch();
            else
                S0 = LocalSearchOPTW();
            for (int i = 0; i < S0.Count(); i++)
            {
                S0_star[i] = S0[i].CopyRoute();
                cons = 1;
            }
            S0_star_score = ComputeScoreRoute(S0_star);
            while ((double)timeLimit > elapsedSeconds)
            {
                iter++;
                Perturbation(noImprovment);
                if (days.Count > 1)
                    S0 = LocalSearch();
                else
                    S0 = LocalSearchOPTW();
                if (S0_score > S0_star_score)
                {
                    for (int i = 0; i < S0.Count(); i++)
                    {
                        S0_star[i] = S0[i].CopyRoute();
                    }
                    S0_star_score = ComputeScoreRoute(S0_star);
                    Console.WriteLine("New best score: {0}!", S0_star_score);
                    noImprovment = 0;
                }
                else
                    noImprovment++;
                if ((noImprovment + 1) % threshold1 == 0)
                {
                    N = new List<POI>(G.Nodes);
                    N.Remove(G.Nodes[0]);
                    N.Remove(G.Nodes[G.Nodes.Count - 1]);
                    N_star.Clear();
                    for (int i = 0; i < S0_star.Count(); i++)
                    {
                        foreach (POI act_poi in S0_star[i].RouteList)
                        {
                            N.Remove(act_poi);
                            N_star.Add(act_poi);
                        }
                        S0[i] = S0_star[i].CopyRoute();
                    }
                }
                if (iter % 2 == 0)
                    cons++;
                elapsed = watch.Elapsed;
                elapsedSeconds = elapsed.TotalSeconds;
            }
        }
    }

    /// <summary>
    /// Class representing SAILS algorithm.
    /// </summary>
    internal class SAILS : ILSBase
    {
        private double t0;
        private double alpha;
        private double maxInnerLoop;
        private Route[] S0_apostrophe;
        private double S0_apostrophe_score;
        private int limit;
        /// <summary>
        /// Constructor of the class.
        /// </summary>
        public SAILS() { }
        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="g">Graph of the problem.</param>
        /// <param name="days">List of ints representing days of week of the trip.</param>
        /// <param name="threshold2">Parameter affecting perturbation.</param>
        /// <param name="threshold3">Parameter affecting perturbation.</param>
        /// <param name="f">Parameter for selecting best NPMs to choose one randomly.</param>
        /// <param name="t0">Initial value of temperature.</param>
        /// <param name="alpha">Cooling parameter.</param>
        /// <param name="maxInnerLoop">Maximal value of inner loop.</param>
        /// <param name="limit">Parameter affecting when to search the actuarl best solution.</param>
        /// <param name="startNode">Starting point of the problem.</param>
        /// <param name="endNode">End point of the trip.</param>
        public SAILS(Graph g, List<int> days, int threshold2, int threshold3, int f, double t0, double alpha, double maxInnerLoop, int limit, POI startNode, POI endNode)
        {
            G = g;
            exchangeInd = days.Count - 1;
            S0 = new Route[days.Count];
            S0_apostrophe = new Route[days.Count];
            for (int i = 0; i < days.Count; i++)
            {
                S0[i] = new Route(days[i]);
            }
            N = g.Nodes.ToList();
            N.Remove(N[0]);
            N.Remove(N[N.Count - 1]);
            N_star = new List<POI>();
            cons = 1;
            post = 0;
            threshold1 = 0;
            this.threshold2 = threshold2;
            this.threshold3 = threshold3;
            this.f = f;
            S0_score = 0;
            S0_star_score = 0;
            S0_star = new Route[days.Count];
            iter = 0;
            this.startNode = startNode;
            this.endNode = endNode;
            this.days = days;
            check = new int[6] { 0, 0, 0, 0, 0, 0 };
            this.t0 = t0;
            this.alpha = alpha;
            this.maxInnerLoop = maxInnerLoop;
            this.limit = limit;
        }
        /// <summary>
        /// Method for starting the algorithm SAILS.
        /// </summary>
        /// <param name="timeLimit">Time budget for the equatation.</param>
        /// <returns>Returns best found solution.</returns>
        public async Task StartSAILS(int timeLimit)
        {
            int noImprovment = 0;
            int innerLoop;
            Random random = new Random();
            double r;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            var elapsed = watch.Elapsed;
            double elapsedSeconds = elapsed.TotalSeconds;
            double delta;
            double temp = t0;
            S0 = StartConstruction();
            if (days.Count > 1)
                S0 = LocalSearch();
            else
                S0 = LocalSearchOPTW();
            for (int i = 0; i < S0.Count(); i++)
            {
                S0_star[i] = S0[i].CopyRoute();
                S0_apostrophe[i] = S0[i].CopyRoute();
            }

            S0_star_score = ComputeScoreRoute(S0_star);
            S0_score = S0_star_score;
            S0_apostrophe_score = S0_star_score;
            Console.WriteLine("Starting score: {0}", S0_star_score);
            while ((double)timeLimit > elapsedSeconds)
            //while(true)
            {
                innerLoop = 0;
                while (innerLoop < maxInnerLoop)
                {
                    iter++;
                    Perturbation(noImprovment);
                    if (days.Count > 1)
                        S0 = LocalSearch();
                    else
                        S0 = LocalSearchOPTW();
                    delta = S0_score - S0_apostrophe_score;
                    if (delta > 0)
                    {
                        for (int i = 0; i < S0.Count(); i++)
                        {
                            S0_apostrophe[i] = S0[i].CopyRoute();
                        }
                        if (S0_score > S0_star_score)
                        {
                            //Console.WriteLine("New best score: {0}!", S0_score);
                            for (int i = 0; i < S0.Count(); i++)
                            {
                                S0_star[i] = S0[i].CopyRoute();
                            }
                            S0_star_score = ComputeScoreRoute(S0_star);
                            Console.WriteLine("New best score: {0}!", S0_star_score);
                            noImprovment = 0;
                        }
                        else
                            noImprovment++;
                    }
                    else
                    {
                        r = random.NextDouble();
                        if (r < Math.Exp(delta / temp))
                        {
                            for (int i = 0; i < S0.Count(); i++)
                            {
                                S0_apostrophe[i] = S0[i].CopyRoute();
                            }
                        }
                        else
                        {
                            N = new List<POI>(G.Nodes);
                            N_star.Clear();
                            N.Remove(G.Nodes[0]);
                            N.Remove(G.Nodes[G.Nodes.Count - 1]);
                            for (int i = 0; i < S0.Count(); i++)
                            {
                                S0[i] = S0_apostrophe[i].CopyRoute();
                                foreach (POI act_poi in S0_apostrophe[i].RouteList)
                                {
                                    N.Remove(act_poi);
                                    N_star.Add(act_poi);
                                }
                            }
                        }
                        noImprovment++;
                    }
                    innerLoop++;
                }
                temp *= alpha;
                if (noImprovment > limit)
                {
                    N = new List<POI>(G.Nodes);
                    N.Remove(G.Nodes[0]);
                    N.Remove(G.Nodes[G.Nodes.Count - 1]);
                    N_star.Clear();
                    for (int i = 0; i < S0_star.Count(); i++)
                    {
                        foreach (POI act_poi in S0_star[i].RouteList)
                        {
                            N.Remove(act_poi);
                            N_star.Add(act_poi);
                        }
                        S0[i] = S0_star[i].CopyRoute();
                        S0_apostrophe[i] = S0_star[i].CopyRoute();
                    }
                    noImprovment = 0;
                }
                if (iter % 2 == 0)
                    cons++;
                if (iter % 200 == 0)
                {
                    S0_star_score = ComputeScoreRoute(S0_star);
                    Console.WriteLine("Iter: {0}. Actual score: {1}", iter, S0_star_score);
                }
                elapsed = watch.Elapsed;
                elapsedSeconds = elapsed.TotalSeconds;
            }
        }
    }

    /// <summary>
    /// Class representing experimental algorithm SAILSACS.
    /// </summary>
    internal class SAILSACS : ILSBase
    {
        private double t0;
        private double alpha;
        private double maxInnerLoop;
        private Route[] S0_apostrophe;
        private double S0_apostrophe_score;
        private int limit;
        private double rho;
        private double psi;
        private double q0;
        private int antN;
        private double pheromone;

        /// <summary>
        /// Constructor of the class.
        /// </summary>
        public SAILSACS() { }
        /// <summary>
        /// Constructor of the class.
        /// </summary>
        /// <param name="g">Graph of the problem.</param>
        /// <param name="days">List of ints representing days of week of the trip.</param>
        /// <param name="threshold2">Parameter affecting perturbation.</param>
        /// <param name="threshold3">Parameter affecting perturbation.</param>
        /// <param name="f">Parameter for selecting best NPMs to choose one randomly.</param>
        /// <param name="t0">Initial value of temperature.</param>
        /// <param name="alpha">Cooling parameter.</param>
        /// <param name="maxInnerLoop">Maximal value of inner loop.</param>
        /// <param name="limit">Parameter affecting when to search the actuarl best solution.</param>
        /// <param name="antN">Number of ants.</param>
        /// <param name="rho">Parameter affecting global update.</param>
        /// <param name="psi">Parameter affecting local update.</param>
        /// <param name="q0">Value representing the chance to which method use during state transition.</param>
        /// <param name="pheromone">Initial Pheromone level.</param>
        /// <param name="startNode">Starting node of the trips.</param>
        /// <param name="endNode">End node of the trips.</param>
        public SAILSACS(Graph g, List<int> days, int threshold2, int threshold3, int f, double t0, double alpha, double maxInnerLoop, int limit, int antN, double rho, double psi, double q0, double pheromone, POI startNode, POI endNode)
        {
            G = g;
            exchangeInd = days.Count - 1;
            S0 = new Route[days.Count];
            S0_apostrophe = new Route[days.Count];
            for (int i = 0; i < days.Count; i++)
            {
                S0[i] = new Route(days[i]);
            }
            N = g.Nodes.ToList();
            N.Remove(N[0]);
            N.Remove(N[N.Count - 1]);
            N_star = new List<POI>();
            cons = 1;
            post = 0;
            threshold1 = 0;
            this.threshold2 = threshold2;
            this.threshold3 = threshold3;
            this.f = f;
            S0_score = 0;
            S0_star_score = 0;
            S0_star = new Route[days.Count];
            iter = 0;
            this.startNode = startNode;
            this.endNode = endNode;
            this.days = days;
            check = new int[6] { 0, 0, 0, 0, 0, 0 };
            this.t0 = t0;
            this.alpha = alpha;
            this.maxInnerLoop = maxInnerLoop;
            this.limit = limit;
            this.antN = antN;
            this.rho = rho;
            this.psi = psi;
            this.q0 = q0;
            this.pheromone = pheromone;
        }

        /// <summary>
        /// Methor for starting algorithm.
        /// </summary>
        /// <param name="timeLimit">Time budget for running the algorithm.</param>
        /// <param name="antTimeLimit">To be added.</param>
        /// <returns>Best found solution.</returns>
        public async Task StartSAILS(int timeLimit, int antTimeLimit)
        {
            int noImprovment = 0;
            int innerLoop;
            Random random = new Random();
            double r;
            Stopwatch watch = new Stopwatch();
            double delta;
            double temp = t0;
            //S0 = StartConstruction();
            ACO aco = new(5, G.Nodes, startNode, endNode, days);
            double newTimeLimitAnt = timeLimit / 4;
            antTimeLimit = (int)Math.Round(newTimeLimitAnt);
            timeLimit = timeLimit - antTimeLimit;
            aco.StartACO(antTimeLimit, pheromone, rho, psi, q0);
            List<AntRoute> results = aco.BestRoute;
            for (int i =0; i < results.Count; i++)
            {
                S0[i].RouteList = results[i].RouteList.ToList();
                S0[i].Score = results[i].Score;
                S0[i].DayNum = results[i].DayNum;
                S0[i].Length = results[i].Length;
            }
            S0_score = aco.BestScore;
            foreach (Route route in S0)
            {
                foreach (POI poi in route.RouteList)
                {
                    N.Remove(poi);
                    N_star.Add(poi);
                }
            }
            watch.Start();
            var elapsed = watch.Elapsed;
            double elapsedSeconds = elapsed.TotalSeconds;
            
            for (int i = 0; i < S0.Count(); i++)
            {
                S0_star[i] = S0[i].CopyRoute();
                S0_apostrophe[i] = S0[i].CopyRoute();
            }

            Console.WriteLine("S0_score {0}: ", S0_score);
            S0_star_score = ComputeScoreRoute(S0_star);
            S0_score = S0_star_score;
            S0_apostrophe_score = S0_star_score;
            Console.WriteLine("Starting score: {0}", S0_star_score);
            while (true)
            //while(true)
            {
                innerLoop = 0;
                while (innerLoop < maxInnerLoop)
                {
                    iter++;
                    Perturbation(noImprovment);
                    if (days.Count == 1)
                        S0 = LocalSearchOPTW();
                    else
                    {
                        S0 = LocalSearch();
                    } 
                    
                    delta = S0_score - S0_apostrophe_score;
                    if (delta > 0)
                    {
                        for (int i = 0; i < S0.Count(); i++)
                        {
                            S0_apostrophe[i] = S0[i].CopyRoute();
                        }
                        if (S0_score > S0_star_score)
                        {
                            //Console.WriteLine("New best score: {0}!", S0_score);
                            for (int i = 0; i < S0.Count(); i++)
                            {
                                S0_star[i] = S0[i].CopyRoute();
                            }
                            S0_star_score = ComputeScoreRoute(S0_star);
                            Console.WriteLine("New best score: {0}!", S0_star_score);
                            noImprovment = 0;
                        }
                        else
                            noImprovment++;
                    }
                    else
                    {
                        r = random.NextDouble();
                        if (r < Math.Exp(delta / temp))
                        {
                            for (int i = 0; i < S0.Count(); i++)
                            {
                                S0_apostrophe[i] = S0[i].CopyRoute();
                            }
                        }
                        else
                        {
                            N = new List<POI>(G.Nodes);
                            N_star.Clear();
                            N.Remove(G.Nodes[0]);
                            N.Remove(G.Nodes[G.Nodes.Count - 1]);
                            for (int i = 0; i < S0.Count(); i++)
                            {
                                S0[i] = S0_apostrophe[i].CopyRoute();
                                foreach (POI act_poi in S0_apostrophe[i].RouteList)
                                {
                                    N.Remove(act_poi);
                                    N_star.Add(act_poi);
                                }
                            }
                        }
                        noImprovment++;
                    }
                    innerLoop++;
                }
                temp *= alpha;
                if (noImprovment > limit)
                {
                    N = new List<POI>(G.Nodes);
                    N.Remove(G.Nodes[0]);
                    N.Remove(G.Nodes[G.Nodes.Count - 1]);
                    N_star.Clear();
                    for (int i = 0; i < S0_star.Count(); i++)
                    {
                        foreach (POI act_poi in S0_star[i].RouteList)
                        {
                            N.Remove(act_poi);
                            N_star.Add(act_poi);
                        }
                        S0[i] = S0_star[i].CopyRoute();
                        S0_apostrophe[i] = S0_star[i].CopyRoute();
                    }
                    noImprovment = 0;
                }
                if (iter % 2 == 0)
                    cons++;
                if (iter % 200 == 0)
                {
                    S0_star_score = ComputeScoreRoute(S0_star);
                    Console.WriteLine("Iter: {0}. Actual score: {1}", iter, S0_star_score);
                }
                elapsed = watch.Elapsed;
                elapsedSeconds = elapsed.TotalSeconds;
                if (elapsedSeconds > timeLimit)
                    break;
            }
        }
    }
}


