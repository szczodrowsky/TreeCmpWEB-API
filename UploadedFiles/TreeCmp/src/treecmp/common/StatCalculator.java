/** This file is part of TreeCmp, a tool for comparing phylogenetic trees    using the Matching Split distance and other metrics.    Copyright (C) 2011,  Damian Bogdanowicz    This program is free software: you can redistribute it and/or modify    it under the terms of the GNU General Public License as published by    the Free Software Foundation, either version 3 of the License, or    (at your option) any later version.    This program is distributed in the hope that it will be useful,    but WITHOUT ANY WARRANTY; without even the implied warranty of    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the    GNU General Public License for more details.    You should have received a copy of the GNU General Public License    along with this program.  If not, see <http://www.gnu.org/licenses/>. */package treecmp.common;import java.util.ArrayList;import java.util.HashSet;import java.util.List;import java.util.Set;import pal.misc.IdGroup;import pal.tree.SimpleTree;import pal.tree.Tree;import pal.tree.TreeRestricter;import pal.tree.TreeUtils;import treecmp.config.IOSettings;import treecmp.metric.Alignment;import treecmp.metric.BaseMetric;import treecmp.metric.Metric;import treecmp.statdata.IMetircDistrbHolder;import treecmp.statdata.IMetricDistribution;public class StatCalculator implements Metric, Alignment  { /** Creates a new instance of StatCalculator */    private final String ERR_DIFF_LEAVES = "Error. Trees contain differsnt leaf sets.\n"            + "In order to compare such trees run TreeCmp with -P (prune) option enabeld.";    private Metric met;    private boolean findMaxDistTrees;    private boolean findMinDistTrees;    private ArrayList< Tree[]> maxDistTrees;    private ArrayList< Tree[]> minDistTrees;    private int maxTreesListSize;    private int matrixDim;        private  ArrayList< Double> valuesHolder;    protected int count;    protected double sum;    protected double sq_sum;    protected double min;    protected double max;        protected boolean recordValues;    protected List<String> commonIdsList;    public boolean isRecordValues() {        return recordValues;    }    public void setRecordValues(boolean recordValues) {        this.recordValues = recordValues;    }    private Tree treeT1;    private Tree treeT2;    private boolean pruneTrees;    private boolean randomComparison;    private int commonTaxaNum;    private int t1TaxaNum;    private int t2TaxaNum;        private int lastN;    private double lastUnifAvg;    private double lastYuleAvg;    private double lastDist;    private static final IOSettings settings = IOSettings.getIOSettings();    public double getLastDist() {        return lastDist;    }    public int getCommonTaxaNum() {        return commonTaxaNum;    }    public int getT1TaxaNum() {        return t1TaxaNum;    }    public int getT2TaxaNum() {        return t2TaxaNum;    }        public Tree getTreeT1() {        return treeT1;    }    public Tree getTreeT2() {        return treeT2;    }       public boolean isPruneTrees() {        return pruneTrees;    }    public boolean isRandomComparison() {        return randomComparison;    }        public StatCalculator() {                this.count=0;        this.sum=0.0;        this.sq_sum=0.0;                this.min=Double.MAX_VALUE;        this.max=Double.MIN_VALUE;        this.findMaxDistTrees=false;        this.findMinDistTrees=false;        maxDistTrees= new ArrayList< Tree[]>();        minDistTrees= new ArrayList< Tree[]>();        valuesHolder=new ArrayList< Double>();        this.maxTreesListSize = Integer.MAX_VALUE;        this.recordValues = false;                this.lastDist = -1;        this.lastN = -1;        this.lastUnifAvg = Double.NEGATIVE_INFINITY;        this.lastYuleAvg = Double.NEGATIVE_INFINITY;                this.pruneTrees = settings.isPruneTrees();        this.randomComparison = settings.isRandomComparison();    }        /**     * This feature is disabled by default.      * @param _findExtremalTrees     */    public void setFindMaxDistTrees(boolean _findMaxDistTrees)    {       this.findMaxDistTrees=_findMaxDistTrees;    }    public void setMaxTreesListSize(int maxTreesListSize) {        this.maxTreesListSize = maxTreesListSize;    }        /**    * This feature is disabled by default.     * @param _findMinDistTrees    */     public void setFindMinDistTrees(boolean _findMinDistTrees)    {       this.findMinDistTrees=_findMinDistTrees;    }    /**     *      * @param _met     */    public StatCalculator(Metric _met) {              //call non-parameter constructor        this();              this.met=_met;    }    public StatCalculator(Metric _met, int _matrixDim)    {        this(_met);        matrixDim=_matrixDim;    }    public void addMetric(Metric _met)    {        this.met=_met;        this.clear();            }           public void clear()  {        this.count=0;        this.sum=0.0;        this.sq_sum=0.0;                this.min=Double.MAX_VALUE;        this.max=Double.MIN_VALUE;                this.maxDistTrees.clear();        this.minDistTrees.clear();        this.valuesHolder.clear();    }    public ArrayList<Tree[]> getMaxDistTrees() {        return maxDistTrees;    }    public ArrayList<Tree[]> getMinDistTrees() {        return minDistTrees;    }  public double getMax()  {   return this.max;  }  public double getMin()  {    return this.min;  }  public double getAvg()  {      double avg=Double.POSITIVE_INFINITY;      if (count>0)           avg=this.sum/(double)count;            return avg;  }    public double getVariance()  {      double var=Double.POSITIVE_INFINITY;      double avg;      if (count>0)      {          avg=this.getAvg();          var=this.sq_sum/(double)count-avg*avg;      }      return var;  }    public double getStd()  {        double std=Double.POSITIVE_INFINITY;        double var;                if(count>0)        {            var=this.getVariance();            std=Math.sqrt(var);                            }        return std;  }       public int getCount()  {      return this.count;  }                        public double getDistance(Tree t1, Tree t2) throws TreeCmpException {                Tree t1Local = t1;        Tree t2Local = t2;        int n;        double dist = -1;                //it also fills commonIdsList        boolean isSameLaves = isLeafSetsEqual(t1,t2);                if (!isSameLaves && !pruneTrees){            throw new TreeCmpException(ERR_DIFF_LEAVES);           }        if (! (met.isDiffLeafSets() && settings.isUseMsMcFreeLeafSet())){            if (pruneTrees) {                Tree [] prunedTrees = pruneTrees(t1,t2);                t1Local = prunedTrees[0];                t2Local = prunedTrees[1];            }        }        n = t1Local.getExternalNodeCount();        if (randomComparison){           if (n != lastN){                lastN = n;                updateRandomData();            }        }       if (n <=1)            return 0;        else if (!met.isRooted() && n > 2) {            //unroot trees for metrics for unrooted trees            t1Local = TreeCmpUtils.unrootTreeIfNeeded(t1Local);            t2Local = TreeCmpUtils.unrootTreeIfNeeded(t2Local);                    }        treeT1 = t1Local;        treeT2 = t2Local;        if (met.isWeighted()) {            dist = met.getDistance(t1Local, t2Local);        }        else {            if (met.isRooted() && n <= 2)                dist = 0;            else if (!met.isRooted() && n <= 3)                dist = 0;            else                dist = met.getDistance(t1Local, t2Local);        }        sum+=dist;        count++;        sq_sum+=dist*dist;                if (this.findMaxDistTrees)            updateMaxTrees(dist,t1,t2);                if (this.findMinDistTrees)            updateMinTrees(dist,t1,t2);                if(dist<min) min=dist;        if(dist>max) max=dist;        if(this.recordValues)            addValue(dist);        lastDist = dist;        return dist;    }    private void updateRandomData(){        //it should be better organized        IMetircDistrbHolder unifomRandData = ((BaseMetric)met).getUnifomRandData();              if (unifomRandData != null){            IMetricDistribution unifDist = unifomRandData.getDistribution(lastN);            if (unifDist != null)                lastUnifAvg = unifDist.getAvg();            else                lastUnifAvg = Double.NEGATIVE_INFINITY;        }        IMetircDistrbHolder yuleRandData = ((BaseMetric)met).getYuleRandData();        if (yuleRandData != null){            IMetricDistribution yuleDist = yuleRandData.getDistribution(lastN);            if (yuleDist != null)                lastYuleAvg = yuleDist.getAvg();            else                lastYuleAvg = Double.NEGATIVE_INFINITY;        }    }    public String getName() {        return this.met.getName();    }     public String getCommandLineName() {        return this.met.getCommandLineName();    }    public void setCommandLineName(String commandLineName) {        this.met.setCommandLineName(commandLineName);    }    public void setName(String name) {        this.met.setName(name);    }    public String getDescription() {        return this.met.getDescription();    }    public void setDescription(String description) {        this.met.setDescription(description);    }    private List<String> getCommonLeaves(Tree t1, Tree t2){        IdGroup idGroup1 = TreeUtils.getLeafIdGroup(t1);        IdGroup idGroup2 = TreeUtils.getLeafIdGroup(t2);        Set<String> id1Set = new HashSet<String> ((idGroup1.getIdCount()*4)/3);        for(int i = 0; i < idGroup1.getIdCount(); i++){            id1Set.add(idGroup1.getIdentifier(i).getName());        }        List<String> commonIds = new ArrayList(idGroup1.getIdCount());        for(int i = 0; i < idGroup2.getIdCount(); i++){            String name = idGroup2.getIdentifier(i).getName();            if (id1Set.contains(name))                commonIds.add(name);        }        return commonIds;    }   private  boolean isLeafSetsEqual(Tree t1, Tree t2){        IdGroup idGroup1 = TreeUtils.getLeafIdGroup(t1);        IdGroup idGroup2 = TreeUtils.getLeafIdGroup(t2);        commonIdsList = getCommonLeaves(t1,t2);        commonTaxaNum = commonIdsList.size();        t1TaxaNum = idGroup1.getIdCount();        t2TaxaNum = idGroup2.getIdCount();        if (t1TaxaNum == commonTaxaNum && t2TaxaNum == commonTaxaNum)            return true;        else            return false;                    }    private Tree[] pruneTrees(Tree t1, Tree t2){               // IdGroup idGroup1 = TreeUtils.getLeafIdGroup(t1);       // IdGroup idGroup2 = TreeUtils.getLeafIdGroup(t2);                      //List<String> commonIdsList = getCommonLeaves(t1,t2);        String [] commonIdsArray = commonIdsList.toArray(new String[0]);                      Tree [] trees = new Tree[2];        if (commonTaxaNum > 0){            if (t1TaxaNum == commonTaxaNum && t2TaxaNum == commonTaxaNum){                trees[0] = t1;                trees[1] = t2;            }else {                TreeRestricter tr1 = new TreeRestricter(t1, commonIdsArray, true);                TreeRestricter tr2 = new TreeRestricter(t2, commonIdsArray, true);                trees[0] = tr1.generateTree();                trees[1] = tr2.generateTree();            }        }        else{            //empty trees             trees[0] = new SimpleTree();             trees[1] = new SimpleTree();        }        return trees;    }    private void updateMaxTrees(double dist, Tree t1, Tree t2)    {             Tree[] treePair=new Tree[2];        treePair[0]=t1;        treePair[1]=t2;                if(dist==max&& this.maxDistTrees.size()<this.maxTreesListSize)        {            //add pair to maxDist list            this.maxDistTrees.add(treePair);                    }else if(dist>max)        {            this.maxDistTrees.clear();            this.maxDistTrees.add(treePair);                }                }        private void updateMinTrees(double dist, Tree t1, Tree t2)    {             Tree[] treePair=new Tree[2];        treePair[0]=t1;        treePair[1]=t2;                            if(dist==min && this.minDistTrees.size()<this.maxTreesListSize)        {            //add pair to minDist list            this.minDistTrees.add(treePair);                    }else if(dist<min)        {            this.minDistTrees.clear();            this.minDistTrees.add(treePair);                }           }    protected void addValue(double dist)    {        this.valuesHolder.add(dist);    }    public ArrayList<Double> getValuesHolder() {        return valuesHolder;    }    public void initData() {        met.initData();    }    public double getLastDistToYuleAvg() {        if (lastYuleAvg == Double.NEGATIVE_INFINITY)            return Double.NEGATIVE_INFINITY;        else            //return (lastDist*100.0)/lastYuleAvg;            return lastDist/lastYuleAvg;    }     public double getLastDistToUnifAvg() {        if (lastUnifAvg == Double.NEGATIVE_INFINITY)            return Double.NEGATIVE_INFINITY;        else            return lastDist/lastUnifAvg;            //return (lastDist*100.0)/lastUnifAvg;    }    public AlignInfo getAlignment() {        int n = treeT1.getExternalNodeCount();                if (met.isRooted() && n <=2)            return null;        else if (!met.isRooted() && n <=3)             return null;        return met.getAlignment();    }    public String getAlnFileSuffix() {        return ((BaseMetric)met).getAlnFileSuffix();    }    public boolean isRooted(){        return met.isRooted();    }    public boolean isWeighted(){        return met.isWeighted();    }    public boolean isDiffLeafSets(){        return met.isDiffLeafSets();    }}