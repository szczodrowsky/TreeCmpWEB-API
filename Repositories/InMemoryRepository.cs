﻿using TreeCmpWebAPI.Models.Domain;

namespace TreeCmpWebAPI.Repositories
{
    public class InMemoryRepository : INewickRepositories
    {
        public Task<Newick> CreateAsync(Newick newick)
        {
            throw new NotImplementedException();
        }

        public Task<Newick?> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public  async Task<List<Newick>> GetAllAsync()
        {
            return new List<Newick>
             {
                new Newick()
                {
                    Id = Guid.NewGuid(),
                    comparisionMode = "-w",
                    newickFirstString = @"#nexus
                                        begin trees;
                                          tree_one = (15:0.100000,(10:0.100000,(((((12:0.100000,(14:0.100000,4:0.100000):0.100000):0.100000,13:0.100000):0.100000,9:0.100000):0.100000,(2:0.100000,11:0.100000):0.100000):0.100000,(6:0.100000,(8:0.100000,((7:0.100000,5:0.100000):0.100000,3:0.100000):0.100000):0.100000):0.100000):0.100000,1:0.100000);
                                          tree_two = ((4:0.150544,((((11:0.065873,13:0.116382):0.033734,12:0.030332):0.040636,15:0.100000):0.033699,14:0.191842):0.130937):0.054639,((8:0.062614,(7:0.125118,(10:0.112016,9:0.148703):0.031571):0.249102):0.041360,((3:0.120344,2:0.100000):0.040487,(5:0.198900,6:0.133571):0.076557):0.044130):0.033153,1:0.191819);
                                          tree_three = ((3:0.088858,2:0.100767):0.025391,((6:0.211435,(4:0.091527,5:0.125414):0.021078):0.075315,(((8:0.081965,((((11:0.040791,13:0.116382):0.058415,12:0.032032):0.042032,15:0.100000):0.027826,14:0.083684):0.092429):0.053057,(10:0.112651,9:0.125479):0.148942):0.044990,7:0.125118):0.075954):0.113318,1:0.256537);
                                          tree_four = ((3:0.094620,2:0.124501):0.023625,((6:0.176744,(4:0.062987,5:0.082446):0.081879):0.074894,(((8:0.081965,(15:0.078628,(((11:0.040791,13:0.078948):0.026857,12:0.021683):0.030356,14:0.074846):0.027642):0.110277):0.038606,(10:0.126379,9:0.125479):0.140897):0.053657,7:0.174578):0.078200):0.065308,1:0.770047);
                                          tree_five = (((6:0.144838,(4:0.056811,5:0.067155):0.103577):0.053721,(((8:0.081965,(10:0.095842,9:0.126998):0.085727):0.032698,(15:0.075916,(((11:0.003119,13:0.078948):0.065246,12:0.021683):0.030678,14:0.068527):0.022510):0.119957):0.017605,7:0.174578):0.097452):0.078951,(3:0.065666,2:0.198511):0.019047,1:0.770047);
                                        end;",
                    windowWidth = "2",
                    rootedMetrics = [],
                    unrootedMetrics = []
                }
             };
        }

        public Task<Newick?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task SaveOutputFileAsync(NewickResponseFile newickResponseFile)
        {
            throw new NotImplementedException();
        }

        public Task<Newick?> Update(Guid id, Newick newick)
        {
            throw new NotImplementedException();
        }

        public Task<Newick?> UpdateAsync(Guid id, Newick newick)
        {
            throw new NotImplementedException();
        }
    }
}
